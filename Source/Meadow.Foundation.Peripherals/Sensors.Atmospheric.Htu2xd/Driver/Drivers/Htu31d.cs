using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;
using HU = Meadow.Units.RelativeHumidity.UnitType;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide access to the Htu31d
    /// temperature and humidity sensors
    /// </summary>
    public partial class Htu31d : Htux1dBase
    {
        /// <summary>
        /// Create a new Htu31d temperature and humidity sensor.
        /// </summary>
        /// <param name="address">Sensor address (default to 0x40).</param>
        /// <param name="i2cBus">I2CBus (default to 100 KHz).</param>
        public Htu31d(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address)
        {
            SerialNumber = GetSerial();
        }

        /// <summary>
        /// Read atmospheric data from sensor
        /// </summary>
        /// <returns></returns>
        protected async override Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> ReadSensor()
        {
            (Units.Temperature? Temperature, RelativeHumidity? Humidity) conditions;

            BusComms?.Write((byte)Commands.Conversion);
            await Task.Delay(20); // Maximum conversion time is 20ms 
            BusComms?.ReadRegister((byte)Commands.ReadTempHumidity, ReadBuffer.Span[0..5]);// 2 bytes for temp, checksum, 2 bytes humidity, checksum

            var temperatureReading = (ushort)((ReadBuffer.Span[0] << 8) + ReadBuffer.Span[1]);
            conditions.Temperature = new Units.Temperature((float)(((175.72 * temperatureReading) / 65536) - 46.85), Units.Temperature.UnitType.Celsius);

            var humidityReading = (ushort)((ReadBuffer.Span[3] << 8) + ReadBuffer.Span[4]);
            var humidity = (125 * (float)humidityReading / 65536) - 6;
            humidity = Math.Clamp(humidity, 0, 100);
            conditions.Humidity = new RelativeHumidity(humidity, HU.Percent);

            return conditions;
        }

        /// <summary>
        /// Turn the heater on or off
        /// </summary>
        /// <param name="heaterOn">Heater status, true = turn heater on, false = turn heater off</param>
        public void Heater(bool heaterOn)
        {
            if (heaterOn)
            {
                BusComms?.WriteRegister((byte)Commands.HeaterOn, 1);
            }
            else
            {
                BusComms?.WriteRegister((byte)Commands.HeaterOff, 1);
            }
        }

        /// <summary>
        /// Reset the sensor
        /// </summary>
        public void Reset()
        {
            BusComms?.WriteRegister((byte)Commands.Reset, 1);

            Thread.Sleep(15); //could make this async ...
        }

        private UInt32 GetSerial()
        {
            if (BusComms == null) return 0;

            var data = new byte[4];

            BusComms.ReadRegister((byte)Commands.ReadSerial, data);

            UInt32 serial;

            serial = data[0];
            serial <<= 8;
            serial |= data[1];
            serial <<= 8;
            serial |= data[2];
            serial <<= 8;
            serial |= data[3];
            return serial;
        }
    }
}