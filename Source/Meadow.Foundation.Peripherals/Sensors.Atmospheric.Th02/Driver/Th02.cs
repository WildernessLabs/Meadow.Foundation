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
    /// Represents a TH02 temperature and relative humidity sensor by Seeed Studio
    /// Found in the Grove Temperature and Humidity Sensor (High-Accuracy and Mini)
    /// </summary>
    public partial class Th02 :
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
        /// Provide a mechanism for reading the temperature and humidity from
        /// a Th02 temperature / humidity sensor
        /// </summary>
        public Th02(II2cBus i2cBus, byte address = (byte)Addresses.Default)
                : base(i2cBus, address)
        { }

        /// <summary>
        /// Raise all change events for subscribers
        /// </summary>
        /// <param name="changeResult">temperature and humidity</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> changeResult)
        {
            if (changeResult.New.Temperature is { } temperature)
            {
                _temperatureHandlers?.Invoke(this, new ChangeResult<Units.Temperature>(temperature, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Humidity is { } humidity)
            {
                _humidityHandlers?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(humidity, changeResult.Old?.Humidity));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Calculates the compensated pressure and temperature.
        /// </summary>
        protected override async Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> ReadSensor()
        {
            (Units.Temperature? Temperature, RelativeHumidity? Humidity) conditions;

            //  Read the humidity
            BusComms?.WriteRegister((byte)Registers.Config, MeasureHumidity);

            //  Maximum conversion time should be 40ms
            while ((BusComms?.ReadRegister((byte)Registers.Status) & 0x01) > 0)
            {
                await Task.Delay(40);
            }

            byte[] data = new byte[2];

            BusComms?.ReadRegister((byte)Registers.DataHigh, data);
            int temp = data[0] << 8;
            temp |= data[1];
            temp >>= 4;

            conditions.Humidity = new RelativeHumidity(temp / 16.0 - 24);

            //  Read the temperature
            BusComms?.WriteRegister((byte)Registers.Config, MeasureTemperature);

            //  Maximum conversion time should be 40ms
            while ((BusComms?.ReadRegister((byte)Registers.Status) & 0x01) > 0)
            {
                Thread.Sleep(40);
            }

            BusComms?.ReadRegister((byte)Registers.DataHigh, data);
            temp = data[0] << 8;
            temp |= data[1];
            temp >>= 2; //drop the two unused bits (14 bit value)
            conditions.Temperature = new Units.Temperature(temp / 32.0 - 50, Units.Temperature.UnitType.Celsius);

            return conditions;
        }

        async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
            => (await Read()).Temperature!.Value;

        async Task<RelativeHumidity> ISensor<RelativeHumidity>.Read()
            => (await Read()).Humidity!.Value;
    }
}