using System;
using System.Threading;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Temperature;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide a mechanism for reading the temperature and humidity from
    /// a SHT31D temperature / humidity sensor.
    /// </summary>
    /// <remarks>
    /// Readings from the sensor are made in Single-shot mode.
    /// </remarks>
    public class SHT31D : ITemperatureSensor, IHumiditySensor
    {
        #region Constants

        /// <summary>
        ///     Minimum value that should be used for the polling frequency.
        /// </summary>
        public const ushort MinimumPollingPeriod = 100;

        #endregion Constants

        #region Member variables / fields

        /// <summary>
        ///     SH31D sensor communicates using I2C.
        /// </summary>
        private readonly II2cPeripheral _sht31d;

        /// <summary>
        ///     Update interval in milliseconds
        /// </summary>
        private readonly ushort _updateInterval = 100;

        #endregion Member variables / fields

        #region Properties

        /// <summary>
        ///     Get the last humidity reading from the sensor.
        /// </summary>
        /// <remarks>
        ///     The Update method should be called before the data in this property
        ///     contains valid data.
        /// </remarks>
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
        ///     Get the last temperature reading.
        /// </summary>
        /// <remarks>
        ///     The Update method should be called before the data in this property
        ///     contains valid data.
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
        ///     Default constructor (made private to prevent it being called).
        /// </summary>
        private SHT31D()
        {
        }

        /// <summary>
        ///     Create a new SHT31D object.
        /// </summary>
        /// <param name="address">Sensor address (should be 0x44 or 0x45).</param>
        /// <param name="i2cBus">I2cBus (0-1000 KHz).</param>
        /// <param name="updateInterval">Number of milliseconds between samples (0 indicates polling to be used)</param>
        /// <param name="humidityChangeNotificationThreshold">Changes in humidity greater than this value will trigger an event when updatePeriod > 0.</param>
        /// <param name="temperatureChangeNotificationThreshold">Changes in temperature greater than this value will trigger an event when updatePeriod > 0.</param>
        public SHT31D(II2cBus i2cBus, byte address = 0x44, ushort updateInterval = MinimumPollingPeriod,
                        float humidityChangeNotificationThreshold = 0.001F, 
                        float temperatureChangeNotificationThreshold = 0.001F)
        {
            if ((address != 0x44) && (address != 0x45))
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address should be 0x44 or 0x45");
            }
            if (humidityChangeNotificationThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(humidityChangeNotificationThreshold), "Humidity threshold should be >= 0");
            }
            if (temperatureChangeNotificationThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(temperatureChangeNotificationThreshold), "Temperature threshold should be >= 0");
            }
            if ((updateInterval != 0) && (updateInterval < MinimumPollingPeriod))
            {
                throw new ArgumentOutOfRangeException(nameof(updateInterval), "Update period should be 0 or >= than " + MinimumPollingPeriod);
            }

            TemperatureChangeNotificationThreshold = temperatureChangeNotificationThreshold;
            HumidityChangeNotificationThreshold = humidityChangeNotificationThreshold;
            _updateInterval = updateInterval;

            _sht31d = new I2cPeripheral(i2cBus, address);

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
        ///     Get a reading from the sensor and set the Temperature and Humidity properties.
        /// </summary>
        public void Update()
        {
            var data = _sht31d.WriteRead(new byte[] { 0x2c, 0x06 }, 6);
            Humidity = (100 * (float) ((data[3] << 8) + data[4])) / 65535;
            Temperature = ((175 * (float) ((data[0] << 8) + data[1])) / 65535) - 45;
        }

        #endregion
    }
}