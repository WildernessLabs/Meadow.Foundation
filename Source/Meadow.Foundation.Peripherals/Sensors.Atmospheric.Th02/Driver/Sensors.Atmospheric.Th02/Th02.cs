using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Represents a TH02 temperature and relative humidity sensor by Seeed Studio
    /// Found in the Grove Temperature and Humidiy Sensor (High-Accuracy and Mini)
    /// </summary>
    public partial class Th02 :
        ByteCommsSensorBase<(Units.Temperature? Temperature, RelativeHumidity? Humidity)>,
        ITemperatureSensor, IHumiditySensor
    {
        /// <summary>
        /// Event raised when the temperature changes
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        /// <summary>
        /// Event raised when the humidity changes 
        /// </summary>
        public event EventHandler<IChangeResult<RelativeHumidity>> HumidityUpdated = delegate { };

        /// <summary>
        /// Last value read from the Pressure sensor.
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// Last value read from the Pressure sensor.
        /// </summary>
        public RelativeHumidity? Humidity => Conditions.Humidity;

        /// <summary>
        /// Provide a mechanism for reading the temperature and humidity from
        /// a Th02 temperature / humidity sensor
        /// </summary>
        public Th02(II2cBus i2cBus, byte address = (byte)Addresses.Default)
                : base(i2cBus, address)
        {
        }

        /// <summary>
        /// Raise all change events for subscribers
        /// </summary>
        /// <param name="changeResult">temperature and humidity</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> changeResult)
        {
            if (changeResult.New.Temperature is { } temperature)
            {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temperature, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Humidity is { } humidity)
            {
                HumidityUpdated?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(humidity, changeResult.Old?.Humidity));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Calculates the compensated pressure and temperature.
        /// </summary>
        protected override Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> ReadSensor()
        {
            return Task.Run(() =>
            {
                (Units.Temperature? Temperature, RelativeHumidity? Humidity) conditions;

                //  Get the humidity first.
                Peripheral?.WriteRegister((byte)Registers.Config, MeasureHumidity);
                
                //  Maximum conversion time should be 40ms
                while ((Peripheral?.ReadRegister((byte)Registers.Status) & 0x01) > 0)
                {
                    Thread.Sleep(40);
                }

                byte[] data = new byte[2];
                
                Peripheral?.ReadRegister((byte)Registers.DataHigh, data);
                int temp = data[0] << 8;
                temp |= data[1];
                temp >>= 4;

                conditions.Humidity = new RelativeHumidity(temp / 16.0 - 24);

                //  Now get the temperature.
                Peripheral?.WriteRegister((byte)Registers.Config, MeasureTemperature);
                //  Maximum conversion time should be 40ms
                while ((Peripheral?.ReadRegister((byte)Registers.Status) & 0x01) > 0)
                {
                    Thread.Sleep(40);
                }

                Peripheral?.ReadRegister((byte)Registers.DataHigh, data);
                temp = data[0] << 8;
                temp |= data[1];
                temp >>= 2; //drop the two unused bits (14 bit value)
                conditions.Temperature = new Units.Temperature(temp / 32.0 - 50, Units.Temperature.UnitType.Celsius);

                return conditions;
            });
        }
    }
}