using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Temperature;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// Provide the ability to read the temperature from the following sensors:
    /// - TMP35 / 36 / 37
    /// - LM35 / 45
    /// 
    /// Note: This class is not yet implemented.
    /// </summary>
    /// <remarks>
    /// <i>AnalogTemperature</i> provides a method of reading the temperature from
    /// linear analog temperature sensors.  There are a number of these sensors available
    /// including the commonly available TMP and LM series.
    /// Sensors of this type obey the following equation:
    /// y = mx + c
    /// where y is the reading in millivolts, m is the gradient (number of millivolts per
    /// degree centigrade and C is the point where the line would intercept the y axis.
    /// The <i>SensorType</i> enum defines the list of sensors with default settings in the
    /// library.  Unsupported sensors that use the same linear algorithm can be constructed
    /// by setting the sensor type to <i>SensorType.Custom</i> and providing the settings for
    /// the linear calculations.
    /// The default sensors have the following settings:
    /// Sensor              Millivolts at 25C    Millivolts per degree C
    /// TMP35, LM35, LM45       250                     10
    /// TMP36, LM50             750                     10
    /// TMP37                   500                     20
    /// </remarks>
    public class AnalogTemperature : ITemperatureSensor
    {
        #region Local classes
        
        /// <summary>
        ///     Calibration class for new sensor types.  This allows new sensors
        ///     to be used with this class.
        /// </summary>
        /// <remarks>
        ///     The default settings for this object are correct for the TMP35.
        /// </remarks>
        public class Calibration
        {
            /// <summary>
            ///     Sample reading as specified in the product data sheet.
            /// </summary>
            public int SampleReading { get; private set; } = 25;

            /// <summary>
            ///     Millivolt reading the sensor will generate when the sensor
            ///     is at the Samplereading temperature.  This value can be
            ///     obtained from the data sheet. 
            /// </summary>
            public int MillivoltsAtSampleReading { get; private set; } = 250;

            /// <summary>
            ///     Linear change in the sensor output (in millivolts) per 1 degree C
            ///     change in temperature.
            /// </summary>
            public int MillivoltsPerDegreeCentigrade { get; private set; } = 10;

            /// <summary>
            ///     Default constructor.  Create a new Calibration object with default values
            ///     for the properties.
            /// </summary>
            public Calibration()
            {
            }

            /// <summary>
            ///     Create a new Calibration object using the specified values.
            /// </summary>
            /// <param name="sampleReading">Sample reading from the data sheet.</param>
            /// <param name="millivoltsAtSampleReading">Millivolts output at the sample reading (from the data sheet).</param>
            /// <param name="millivoltsPerDegreeCentigrade">Millivolt change per degree centigrade (from the data sheet).</param>
            public Calibration(int sampleReading, int millivoltsAtSampleReading, int millivoltsPerDegreeCentigrade)
            {
                SampleReading = sampleReading;
                MillivoltsAtSampleReading = millivoltsAtSampleReading;
                MillivoltsPerDegreeCentigrade = millivoltsPerDegreeCentigrade;
            }
        }
        
        #endregion Local classes
        
        #region Constants

        /// <summary>
        ///     Minimum value that should be used for the polling frequency.
        /// </summary>
        public const ushort MinimumPollingPeriod = 100;

        #endregion Constants
        
        #region Enums
        
        /// <summary>
        ///     Type of temperature sensor.
        /// </summary>
        public enum KnownSensorType
        {
            Custom,
            TMP35,
            TMP36,
            TMP37,
            LM35,
            LM45,
            LM50
        }

        #endregion Enums

        #region Member variables /fields

        /// <summary>
        ///     Millivolts per degree centigrade for the sensor attached to the analog port.
        /// </summary>
        /// <remarks>
        ///     This will be the gradient of the line y = mx + c.
        /// </remarks>
        private readonly int _millivoltsPerDegreeCentigrade;

        /// <summary>
        ///     Point where the line y = mx +c would intercept the y-axis.
        /// </summary>
        private readonly float _yIntercept;

        /// <summary>
        ///     Update interval in milliseconds
        /// </summary>
        private readonly ushort _updateInterval = 100;

        #endregion Member variables / fields

        #region Properties

        /// <summary>
        ///     Analog port that the temperature sensor is attached to.
        /// </summary>
        /// <value>Analog port connected to the temperature sensor.</value>
        private IAnalogInputPort AnalogPort { get; set; }

        /// <summary>
        ///     Temperature in degrees centigrade.
        /// </summary>
        /// <remarks>
        ///     The temperature is given by the following calculation:
        ///     temperature = (reading in millivolts - yIntercept) / millivolts per degree centigrade
        /// </remarks>
        public float Temperature
        {
            get { return _temperature; }
            private set
            {
                _temperature = value;
                //
                //  Check to see if the change merits raising an event.
                //
                if ((_updateInterval > 0) &&
                    (Math.Abs(_lastNotifiedTemperature - value) >= TemperatureChangeNotificationThreshold))
                {
                    TemperatureChanged(this, new SensorFloatEventArgs(_lastNotifiedTemperature, value));
                    _lastNotifiedTemperature = value;
                }
            }
        }

        private float _temperature;
        private float _lastNotifiedTemperature = 0.0F;

        /// <summary>
        ///     Any changes in the temperature that are greater than the temperature
        ///     threshold will cause an event to be raised when the instance is
        ///     set to update automatically.
        /// </summary>
        public float TemperatureChangeNotificationThreshold { get; set; } = 0.001F;

        #endregion Properties

        #region Events and delegates

        /// <summary>
        ///     Event raised when the temperature change is greater than the 
        ///     TemperatureChangeNotificationThreshold value.
        /// </summary>
        public event SensorFloatEventHandler TemperatureChanged = delegate { };

        #endregion Events and delegates

        #region Constructor(s)

        /// <summary>
        ///     Default constructor, private to prevent this being used.
        /// </summary>
        private AnalogTemperature()
        {
        }

        /// <summary>
        ///     New instance of the AnalogTemperatureSensor class.
        /// </summary>
        /// <param name="analogPin">Analog pin the temperature sensor is connected to.</param>
        /// <param name="sensorType">Type of sensor attached to the analog port.</param>
        /// <param name="calibration">Calibration for the analog temperature sensor.</param>
        /// <param name="updateInterval">Number of milliseconds between samples (0 indicates polling to be used)</param>
        /// <param name="temperatureChangeNotificationThreshold">Changes in temperature greater than this value will trigger an event when updatePeriod > 0.</param>
        public AnalogTemperature(IIODevice device, IPin analogPin, KnownSensorType sensorType, Calibration calibration = null, 
            ushort updateInterval = MinimumPollingPeriod, float temperatureChangeNotificationThreshold = 0.001F)
        {
            if (temperatureChangeNotificationThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(temperatureChangeNotificationThreshold),
                    "Temperature threshold should be >= 0");
            }

            if ((updateInterval != 0) && (updateInterval < MinimumPollingPeriod))
            {
                throw new ArgumentOutOfRangeException(nameof(updateInterval),
                    "Update period should be 0 or >= than " + MinimumPollingPeriod);
            }
            //
            //  If the calibration object is null use the defaults for TMP35.
            //
            if (calibration == null)
            {
                calibration = new Calibration();
            }

            TemperatureChangeNotificationThreshold = temperatureChangeNotificationThreshold;
            _updateInterval = updateInterval;

            AnalogPort = null; // ToDo: needs device.CreateAnalogInputPort() ..... new AnalogInputPort(analogPin);

            switch (sensorType)
            {
                case KnownSensorType.TMP35:
                case KnownSensorType.LM35:
                case KnownSensorType.LM45:
                    _yIntercept = 0;
                    _millivoltsPerDegreeCentigrade = 10;
                    break;
                case KnownSensorType.LM50:
                case KnownSensorType.TMP36:
                    _yIntercept = 500;
                    _millivoltsPerDegreeCentigrade = 10;
                    break;
                case KnownSensorType.TMP37:
                    _yIntercept = 0;
                    _millivoltsPerDegreeCentigrade = 20;
                    break;
                case KnownSensorType.Custom:
                    _yIntercept = calibration.MillivoltsAtSampleReading - (calibration.SampleReading * calibration.MillivoltsAtSampleReading);
                    _millivoltsPerDegreeCentigrade = calibration.MillivoltsPerDegreeCentigrade;
                    break;
                default:
                    throw new ArgumentException("Unknown sensor type", nameof(sensorType));
            }

            if (updateInterval > 0)
            {
                StartUpdating();
            }
            else
            {
                Update();
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///     Start the update process.
        /// </summary>
        private void StartUpdating()
        {
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    Update();
                    Thread.Sleep(_updateInterval);
                }
            });
            t.Start();
        }

        /// <summary>
        ///     Get the current temperature and update the Temperature property.
        /// </summary>
        public /*async Task*/ void Update()
        {
            // TODO: re-implement
            //float reading = await AnalogPort.Read(1, _updateInterval) * 3300;
            float reading = 0f;
            reading -= _yIntercept;
            Temperature = reading / _millivoltsPerDegreeCentigrade; ;
        }

        #endregion Methods
    }
}