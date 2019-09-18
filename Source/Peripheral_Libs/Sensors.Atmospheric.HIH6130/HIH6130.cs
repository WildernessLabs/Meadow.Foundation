using System;
using System.Threading;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Temperature;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide a mechanism for reading the Temperature and Humidity from
    /// a HIH6130 temperature and Humidity sensor.
    /// </summary>
    public class HIH6130 : ITemperatureSensor, IHumiditySensor
    {
        #region Constants

        /// <summary>
        ///     Minimum value that should be used for the polling frequency.
        /// </summary>
        public const ushort MinimumPollingPeriod = 100;

        #endregion Constants

        #region Member variables / fields

        /// <summary>
        ///     HIH6130 sensor object.
        /// </summary>
        private readonly II2cPeripheral _hih6130;
        
        /// <summary>
        ///     Update interval in milliseconds
        /// </summary>
        private readonly ushort _updateInterval = 100;

        #endregion Member variables / fields

        #region Properties

        /// <summary>
        ///     Humidity reading from the last update.
        /// </summary>
        public float Humidity 
        {
            get { return _humidity; }
            private set
            {
                _humidity = value;
                //
                //  Check to see if the change merits raising an event.
                //
                if ((_updateInterval > 0) && (Math.Abs(_lastNotifiedHumidity - value) >= HumidityChangeNotificationThreshold))
                {
                    HumidityChanged(this, new SensorFloatEventArgs(_lastNotifiedHumidity, value));
                    _lastNotifiedHumidity = value;
                }
            }
        }
        private float _humidity;
        private float _lastNotifiedHumidity = 0.0F;

        /// <summary>
        ///     Temperature reading from last update.
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

        /// <summary>
        ///     Any changes in the humidity that are greater than the humidity
        ///     threshold will cause an event to be raised when the instance is
        ///     set to update automatically.
        /// </summary>
        public float HumidityChangeNotificationThreshold { get; set; } = 0.001F;

        #endregion Properties

        #region Events and delegates

        /// <summary>
        ///     Event raised when the temperature change is greater than the 
        ///     TemperatureChangeNotificationThreshold value.
        /// </summary>
        public event SensorFloatEventHandler TemperatureChanged = delegate { };

        /// <summary>
        ///     Event raised when the humidity change is greater than the
        ///     HumidityChangeNotificationThreshold value.
        /// </summary>
        public event SensorFloatEventHandler HumidityChanged = delegate { };

        #endregion Events and delegates

        #region Constructors

        /// <summary>
        ///     Default constructor is private to prevent the developer from calling it.
        /// </summary>
        private HIH6130()
        {
        }

        /// <summary>
        ///     Create a new HIH6130 object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the HIH6130 (default = 0x27).</param>
        /// <param name="i2cBus">I2C bus (default = 100 KHz).</param>
        /// <param name="updateInterval">Number of milliseconds between samples (0 indicates polling to be used)</param>
        /// <param name="humidityChangeNotificationThreshold">Changes in humidity greater than this value will trigger an event when updatePeriod > 0.</param>
        /// <param name="temperatureChangeNotificationThreshold">Changes in temperature greater than this value will trigger an event when updatePeriod > 0.</param>
        public HIH6130(II2cBus i2cBus, byte address = 0x27, ushort updateInterval = MinimumPollingPeriod,
                       float humidityChangeNotificationThreshold = 0.001F, 
                       float temperatureChangeNotificationThreshold = 0.001F)
        {
            if (humidityChangeNotificationThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(humidityChangeNotificationThreshold), "Humidity threshold should be >= 0");
            }
            if (humidityChangeNotificationThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(temperatureChangeNotificationThreshold), "Temperature threshold should be >= 0");
            }

            if ((updateInterval != 0) && (updateInterval < MinimumPollingPeriod))
            {
                throw new ArgumentOutOfRangeException(nameof(updateInterval), "Update period should be 0 or >= than " + MinimumPollingPeriod);
            }
            _updateInterval = updateInterval;
            HumidityChangeNotificationThreshold = humidityChangeNotificationThreshold;
            TemperatureChangeNotificationThreshold = temperatureChangeNotificationThreshold;

            _hih6130 = new I2cPeripheral(i2cBus, address);

            if (updateInterval > 0)
            {
                StartUpdating();
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
        ///     Force the sensor to make a reading and update the relevant properties.
        /// </summary>
        public void Update()
        {
            _hih6130.WriteByte(0);
            //
            //  Sensor takes 35ms to make a valid reading.
            //
            Thread.Sleep(40);
            var data = _hih6130.ReadBytes(4);
            //
            //  Data format:
            //
            //  Byte 0: S1  S0  H13 H12 H11 H10 H9 H8
            //  Byte 1: H7  H6  H5  H4  H3  H2  H1 H0
            //  Byte 2: T13 T12 T11 T10 T9  T8  T7 T6
            //  Byte 4: T5  T4  T3  T2  T1  T0  XX XX
            //
            if ((data[0] & 0xc0) != 0)
            {
                throw new Exception("Status indicates readings are invalid.");
            }
            var reading = ((data[0] << 8) | data[1]) & 0x3fff;
            Humidity = ((float) reading / 16383) * 100;
            reading = ((data[2] << 8) | data[3]) >> 2;
            Temperature = (((float) reading / 16383) * 165) - 40;
        }

        #endregion Methods
    }
}