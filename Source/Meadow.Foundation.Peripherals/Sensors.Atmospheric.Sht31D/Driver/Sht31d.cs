using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Represents a SHT31 Temperature and humidity sensor
    /// </summary>
    public partial class Sht31d :
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
        /// The temperature from the last reading
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading
        /// </summary>
        public RelativeHumidity? Humidity => Conditions.Humidity;

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Create a new SHT31D object
        /// </summary>
        /// <param name="address">Sensor address (should be 0x44 or 0x45)</param>
        /// <param name="i2cBus">I2cBus (0-1000 KHz).</param>
        public Sht31d(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address, readBufferSize: 6, writeBufferSize: 2)
        { }

        /// <summary>
        /// Raise events for subscribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
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
        /// Get a reading from the sensor and set the Temperature and Humidity properties.
        /// </summary>
        protected override Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> ReadSensor()
        {
            (Units.Temperature Temperature, RelativeHumidity Humidity) conditions;

            // stuff the write buffer
            WriteBuffer.Span[0] = 0x2c;
            WriteBuffer.Span[1] = 0x06;

            BusComms?.Exchange(WriteBuffer.Span, ReadBuffer.Span);

            var humidity = (100 * (float)((ReadBuffer.Span[3] << 8) + ReadBuffer.Span[4])) / 65535;
            var tempC = ((175 * (float)((ReadBuffer.Span[0] << 8) + ReadBuffer.Span[1])) / 65535) - 45;

            conditions.Humidity = new RelativeHumidity(humidity);
            conditions.Temperature = new Units.Temperature(tempC, Units.Temperature.UnitType.Celsius);

            return Task.FromResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity)>(conditions);
        }

        async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
            => (await Read()).Temperature!.Value;

        async Task<RelativeHumidity> ISensor<RelativeHumidity>.Read()
            => (await Read()).Humidity!.Value;
    }
}