using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide a mechanism for reading the temperature and humidity from
    /// a SHT31D temperature / humidity sensor.
    /// </summary>
    /// <remarks>
    /// Readings from the sensor are made in Single-shot mode.
    /// </remarks>
    public class Sht31d :
        ByteCommsSensorBase<(Units.Temperature? Temperature, RelativeHumidity? Humidity)>,
        ITemperatureSensor, IHumiditySensor
    {
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
        public event EventHandler<IChangeResult<RelativeHumidity>> HumidityUpdated = delegate { };

        /// <summary>
        /// The temperature, in degrees celsius (°C), from the last reading.
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public RelativeHumidity? Humidity => Conditions.Humidity;

        /// <summary>
        ///     Create a new SHT31D object.
        /// </summary>
        /// <param name="address">Sensor address (should be 0x44 or 0x45).</param>
        /// <param name="i2cBus">I2cBus (0-1000 KHz).</param>
        public Sht31d(II2cBus i2cBus, byte address = 0x44)
            : base(i2cBus, address, readBufferSize: 6, writeBufferSize: 2)
        {            
        }

        protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> changeResult)
        {
            if (changeResult.New.Temperature is { } temp) {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Humidity is { } humidity) {
                HumidityUpdated?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(humidity, changeResult.Old?.Humidity));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        ///     Get a reading from the sensor and set the Temperature and Humidity properties.
        /// </summary>
        protected override Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> ReadSensor()
        {
            (Units.Temperature Temperature, RelativeHumidity Humidity) conditions;

            // stuff the write buffer
            WriteBuffer.Span[0] = 0x2c;
            WriteBuffer.Span[1] = 0x06;

            Peripheral.Exchange(WriteBuffer.Span, ReadBuffer.Span);

            var humidity = (100 * (float)((ReadBuffer.Span[3] << 8) + ReadBuffer.Span[4])) / 65535;
            var tempC = ((175 * (float)((ReadBuffer.Span[0] << 8) + ReadBuffer.Span[1])) / 65535) - 45;

            conditions.Humidity = new RelativeHumidity(humidity);
            conditions.Temperature = new Units.Temperature(tempC, Units.Temperature.UnitType.Celsius);

            return Task.FromResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity)>(conditions);
        }
    }
}