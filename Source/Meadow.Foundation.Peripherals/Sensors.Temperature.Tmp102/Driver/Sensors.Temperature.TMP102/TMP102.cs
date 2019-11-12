using System;
using System.Threading;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Temperature;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    ///     TMP102 Temperature sensor object.
    /// </summary>    
    public class TMP102 : ITemperatureSensor
    {
        #region Constants

        /// <summary>
        ///     Minimum value that should be used for the polling frequency.
        /// </summary>
        public const ushort MinimumPollingPeriod = 100;

        #endregion Constants

        #region Enums

        /// <summary>
        ///     Indicate the resolution of the sensor.
        /// </summary>
        public enum Resolution : byte
        {
            /// <summary>
            ///     Operate in 12-bit mode.
            /// </summary>
            Resolution12Bits,

            /// <summary>
            ///     Operate in 13-bit mode.
            /// </summary>
            Resolution13Bits
        }

        #endregion Enums

        #region Member variables / fields

        /// <summary>
        ///     TMP102 sensor.
        /// </summary>
        private readonly II2cPeripheral _tmp102;

        /// <summary>
        ///     Update interval in milliseconds
        /// </summary>
        private readonly ushort _updateInterval = 100;

        #endregion Member variables / fields

        #region Properties

        /// <summary>
        ///     Backing variable for the SensorResolution property.
        /// </summary>
        private Resolution _sensorResolution;

        /// <summary>
        ///     Get / set the resolution of the sensor.
        /// </summary>
        public Resolution SensorResolution
        {
            get { return _sensorResolution; }
            set
            {
                var configuration = _tmp102.ReadRegisters(0x01, 2);
                if (value == Resolution.Resolution12Bits)
                {
                    configuration[1] &= 0xef;
                }
                else
                {
                    configuration[1] |= 0x10;
                }
                _tmp102.WriteRegisters(0x01, configuration);
                _sensorResolution = value;
            }
        }

        /// <summary>
        ///     Temperature (in degrees centigrade).
        /// </summary>
        public float Temperature
        {
            get { return _temperature; }
            private set
            {
                _temperature = value;
                //
                //  Check to see if the change merits raising an event.
                //
                if ((_updateInterval > 0) && (Math.Abs(_lastNotifiedTemperature - value) >= TemperatureChangeNotificationThreshold))
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

        #region Constructors

        /// <summary>
        ///     Default constructor (private to prevent it being called).
        /// </summary>
        private TMP102()
        {
        }

        /// <summary>
        ///     Create a new TMP102 object using the default configuration for the sensor.
        /// </summary>
        /// <param name="address">I2C address of the sensor.</param>
        public TMP102(IIODevice device, II2cBus i2cBus, byte address = 0x48, ushort updateInterval = MinimumPollingPeriod,
            float temperatureChangeNotificationThreshold = 0.001F)
        {
            _tmp102 = new I2cPeripheral(i2cBus, address);

            if (temperatureChangeNotificationThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(temperatureChangeNotificationThreshold), "Temperature threshold should be >= 0");
            }
            if ((updateInterval != 0) && (updateInterval < MinimumPollingPeriod))
            {
                throw new ArgumentOutOfRangeException(nameof(updateInterval), "Update period should be 0 or >= than " + MinimumPollingPeriod);
            }

            TemperatureChangeNotificationThreshold = temperatureChangeNotificationThreshold;
            _updateInterval = updateInterval;

            var configuration = _tmp102.ReadRegisters(0x01, 2);
            _sensorResolution = (configuration[1] & 0x10) > 0 ?
                                 Resolution.Resolution13Bits : Resolution.Resolution12Bits;
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
            Thread t = new Thread(() => {
                while (true)
                {
                    Update();
                    Thread.Sleep(_updateInterval);
                }
            });
            t.Start();
        }

        /// <summary>
        ///     Update the Temperature property.
        /// </summary>
        public void Update()
        {
            var temperatureData = _tmp102.ReadRegisters(0x00, 2);
            var sensorReading = 0;
            if (SensorResolution == Resolution.Resolution12Bits)
            {
                sensorReading = (temperatureData[0] << 4) | (temperatureData[1] >> 4);
            }
            else
            {
                sensorReading = (temperatureData[0] << 5) | (temperatureData[1] >> 3);
            }
            Temperature = (float) (sensorReading * 0.0625);
        }

        #endregion Methods
    }
}