using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Represent a SHT4x temperature and humidity sensor (SHT40, SHT41, SHT45, etc.)
    /// </summary>
    public partial class Sht4x :
        ByteCommsSensorBase<(Units.Temperature? Temperature, RelativeHumidity? Humidity)>,
        ITemperatureSensor, IHumiditySensor, II2cPeripheral
    {
        private event EventHandler<IChangeResult<Units.Temperature>> _temperatureHandlers;
        private event EventHandler<IChangeResult<RelativeHumidity>> _humidityHandlers;

        event EventHandler<IChangeResult<Units.Temperature>> ISamplingSensor<Units.Temperature>.Updated
        {
            add => _temperatureHandlers += value;
            remove => _temperatureHandlers -= value;
        }

        event EventHandler<IChangeResult<RelativeHumidity>> ISamplingSensor<RelativeHumidity>.Updated
        {
            add => _humidityHandlers += value;
            remove => _humidityHandlers -= value;
        }

        /// <summary>
        /// Precision of sensor reading
        /// </summary>
        public Precision ReadPrecision { get; protected set; } = Precision.HighPrecisionNoHeat;

        /// <summary>
        /// The current temperature
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The current humidity
        /// </summary>
        public RelativeHumidity? Humidity => Conditions.Humidity;

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Create a new SHT4x object
        /// </summary>
        /// <param name="address">Sensor address</param>
        /// <param name="i2cBus">I2cBus</param>
        public Sht4x(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address, readBufferSize: 6, writeBufferSize: 2)
        { }

        /// <summary>
        /// Raise events for subscribers
        /// </summary>
        /// <param name="changeResult"></param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> changeResult)
        {
            if (changeResult.New.Temperature is { } temp)
            {
                _temperatureHandlers?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Humidity is { } humidity)
            {
                _humidityHandlers?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(humidity, changeResult.Old?.Humidity));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Returns the appropriate delay in ms for the set precision
        /// </summary>
        /// <param name="precision">Precision to calculate delay</param>
        /// <returns></returns>
        protected int GetDelayForPrecision(Precision precision)
        {
            int delay = 10;

            switch (precision)
            {
                case Precision.HighPrecisionNoHeat:
                    delay = 10;
                    break;
                case Precision.MediumPrecisionNoHeat:
                    delay = 5;
                    break;
                case Precision.LowPrecisionNoHeat:
                    delay = 2;
                    break;
                case Precision.HighPrecisionHighHeat1s:
                case Precision.HighPrecisionMediumHeat1s:
                case Precision.HighPrecisionLowHeat1s:
                    delay = 1100;
                    break;
                case Precision.HighPrecisionHighHeat100ms:
                case Precision.HighPrecisionMediumHeat100ms:
                case Precision.HighPrecisionLowHeat100ms:
                    delay = 110;
                    break;
            }

            return delay;
        }

        /// <summary>
        /// Get a reading from the sensor and set the Temperature and Humidity properties.
        /// </summary>
        protected override Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> ReadSensor()
        {
            (Units.Temperature? Temperature, RelativeHumidity? Humidity) conditions;

            BusComms?.Write((byte)ReadPrecision);
            Thread.Sleep(GetDelayForPrecision(ReadPrecision));
            BusComms?.Read(ReadBuffer.Span[0..5]);

            var temperature = (175 * (float)((ReadBuffer.Span[0] << 8) + ReadBuffer.Span[1]) / 65535) - 45;
            var humidity = (125 * (float)((ReadBuffer.Span[3] << 8) + ReadBuffer.Span[4]) / 65535) - 6;

            conditions.Humidity = new RelativeHumidity(humidity);
            conditions.Temperature = new Units.Temperature(temperature, Units.Temperature.UnitType.Celsius);

            return Task.FromResult(conditions);
        }

        async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
            => (await Read()).Temperature!.Value;

        async Task<RelativeHumidity> ISensor<RelativeHumidity>.Read()
            => (await Read()).Humidity!.Value;
    }
}