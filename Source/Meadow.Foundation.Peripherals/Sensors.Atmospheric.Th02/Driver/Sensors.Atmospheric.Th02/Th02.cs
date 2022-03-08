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
        /// Get / set the heater status
        /// </summary>
        public bool HeaterOn
        {
            get => (Peripheral?.ReadRegister((byte)Registers.Config) & HeaterOnBit) > 0;
            
            set
            {
                if (Peripheral == null) return;

                byte config = Peripheral.ReadRegister((byte)Registers.Config);
                if (value)
                    config |= HeaterOnBit;
                else
                    config &= HeaterMask;
                Peripheral?.WriteRegister((byte)Registers.Config, config);
            }
        }

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
        /// a Th02 temperature / humidity sensor.
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

                Console.WriteLine($"A {Peripheral != null}");

                //  Get the humidity first.
                Peripheral?.WriteRegister((byte)Registers.Config, StartMeasurement);
                //  Maximum conversion time should be 40ms but loop just in case it takes longer.
                Thread.Sleep(40);

                Console.WriteLine("B");

            //    while ((Peripheral?.ReadRegister((byte)Registers.Status) & 0x01) > 0);
                byte[] data = new byte[2];
                
                Peripheral?.ReadRegister((byte)Registers.DataHigh, data);
                int temp = data[0] << 8;
                temp |= data[1];
                temp >>= 4;
                Console.WriteLine($"C: {temp}");

                conditions.Humidity = new RelativeHumidity(temp / 16.0 - 24);

                

                //  Now get the temperature.
                Peripheral?.WriteRegister((byte)Registers.Config, StartMeasurement | MeasureTemperature);
                //  Maximum conversion time should be 40ms but loop just in case 
                //  it takes longer.
                Thread.Sleep(40);
              //  while ((Peripheral?.ReadRegister((byte)Registers.Status) & 0x01) > 0);

                Console.WriteLine("D");

                Peripheral?.ReadRegister((byte)Registers.DataHigh, data);
                temp = data[0] << 8;
                temp |= data[1];
                temp >>= 2; //drop the two unused bits (14 bit value)
                conditions.Temperature = new Units.Temperature(temp / 32.0 - 50, Units.Temperature.UnitType.Celsius);

                Console.WriteLine($"E: {temp}");

                return conditions;
            });
        }
    }
}