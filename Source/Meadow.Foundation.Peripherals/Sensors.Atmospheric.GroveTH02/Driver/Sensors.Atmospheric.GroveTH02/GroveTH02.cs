using System;
using System.Threading;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Temperature;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Grove TH02 temperature and humidity sensor.
    /// </summary>
    public class GroveTH02 : ITemperatureSensor, IHumiditySensor
    {
        #region Constants

        /// <summary>
        ///     Start measurement bit in the configuration register.
        /// </summary>
        private const byte StartMeasurement = 0x01;

        /// <summary>
        ///     Measure temperature bit in the configuration register.
        /// </summary>
        private const byte MeasureTemperature = 0x10;

        /// <summary>
        ///     Heater control bit in the configuration register.
        /// </summary>
        private const byte HeaterOnBit = 0x02;

        /// <summary>
        ///     Mask used to turn the heater off.
        /// </summary>
        private const byte HeaterMask = 0xfd;

        /// <summary>
        ///     Minimum value that should be used for the polling frequency.
        /// </summary>
        public const ushort MinimumPollingPeriod = 200;

        #endregion

        #region Class

        /// <summary>
        ///     Register addresses in the Grove TH02 sensor.
        /// </summary>
        private class Registers
        {
            /// <summary>
            ///     Status register.
            /// </summary>
            public const byte Status = 0x00;
            
            /// <summary>
            ///     High byte of the data register.
            /// </summary>
            public const byte DataHigh = 0x01;
            
            /// <summary>
            ///     Low byte of the data register.
            /// </summary>
            public const byte DataLow = 0x02;
            
            /// <summary>
            ///     Addess of the configuration register.
            /// </summary>
            public const byte Config = 0x04;
            
            /// <summary>
            ///     Address of the ID register.
            /// </summary>
            public const byte ID = 0x11;
        }

        #endregion Class

        #region Member variables / fields

        /// <summary>
        ///     GroveTH02 object.
        /// </summary>
        private readonly II2cPeripheral _groveTH02 = null;

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

        /// <summary>
        ///     Get / set the heater status.
        /// </summary>
        public bool HeaterOn
        {
            get
            {
                return ((_groveTH02.ReadRegister(Registers.Config) & HeaterOnBit) > 0);
            }
            set
            {
                byte config = _groveTH02.ReadRegister(Registers.Config);
                if (value)
                {
                    config |= HeaterOnBit;                    
                }
                else
                {
                    config &= HeaterMask;
                }
                _groveTH02.WriteRegister(Registers.Config, config);
            }
        }

        #endregion

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
        private GroveTH02()
        {
        }

        /// <summary>
        ///     Create a new GroveTH02 object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the Grove TH02 (default = 0x4-).</param>
        /// <param name="i2cBus">I2C bus (default = 100 KHz).</param>
        /// <param name="updateInterval">Number of milliseconds between samples (0 indicates polling to be used)</param>
        /// <param name="humidityChangeNotificationThreshold">Changes in humidity greater than this value will trigger an event when updatePeriod > 0.</param>
        /// <param name="temperatureChangeNotificationThreshold">Changes in temperature greater than this value will trigger an event when updatePeriod > 0.</param>
        public GroveTH02(II2cBus i2cBus, byte address = 0x40, ushort updateInterval = MinimumPollingPeriod,
            float humidityChangeNotificationThreshold = 0.001F,
            float temperatureChangeNotificationThreshold = 0.001F)
        {
            _groveTH02 = new I2cPeripheral(i2cBus, address);

            if (humidityChangeNotificationThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(humidityChangeNotificationThreshold), "Humidity threshold should be >= 0");
            }
            if (humidityChangeNotificationThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(temperatureChangeNotificationThreshold), "Temperature threshold should be >= 0");
            }
            TemperatureChangeNotificationThreshold = temperatureChangeNotificationThreshold;
            HumidityChangeNotificationThreshold = humidityChangeNotificationThreshold;
            _updateInterval = updateInterval;
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
        ///     Force the sensor to make a reading and update the relevant properties.
        /// </summary>
        public void Update()
        {
            int temp = 0;
            //
            //  Get the humidity first.
            //
            _groveTH02.WriteRegister(Registers.Config, StartMeasurement);
            //
            //  Maximum conversion time should be 40ms but loop just in case 
            //  it takes longer.
            //
            Thread.Sleep(40);
            while ((_groveTH02.ReadRegister(Registers.Status) & 0x01) > 0) ;
            byte[] data = _groveTH02.ReadRegisters(Registers.DataHigh, 2);
            temp = data[0] << 8;
            temp |= data[1];
            temp >>= 4;
            Humidity = (((float) temp) / 16) - 24;
            //
            //  Now get the temperature.
            //
            _groveTH02.WriteRegister(Registers.Config, StartMeasurement | MeasureTemperature);
            //
            //  Maximum conversion time should be 40ms but loop just in case 
            //  it takes longer.
            //
            Thread.Sleep(40);
            while ((_groveTH02.ReadRegister(Registers.Status) & 0x01) > 0) ;
            data = _groveTH02.ReadRegisters(Registers.DataHigh, 2);
            temp = data[0] << 8;
            temp |= data[1];
            temp >>= 2;
            Temperature = (((float) temp) / 32) - 50;
        }

        #endregion
    }
}
