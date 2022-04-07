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
        /// <param name="updateInterval">Update interval, defaults to 1 sec if null</param>
        public Htu31d(II2cBus i2cBus, byte address = (byte)Addresses.Default, TimeSpan? updateInterval = null)
            : base(i2cBus, address, updateInterval)
        {
            SerialNumber = GetSerial();
        }

        /// <summary>
        /// Read atmospheric data from sensor
        /// </summary>
        /// <returns></returns>
        protected override async Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> ReadSensor()
        {
            (Units.Temperature Temperature, RelativeHumidity Humidity) conditions;

            return await Task.Run(() => 
            {
                Peripheral?.Write((byte)Commands.Conversion);
                Thread.Sleep(20); // Maximum conversion time is 20ms 
                Peripheral?.ReadRegister((byte)Commands.ReadTempHumidity, ReadBuffer.Span[0..5]);// 2 bytes for temp, checksum, 2 bytes humidity, checksum

                // temperature
                var temperatureReading = (ushort)((ReadBuffer.Span[0] << 8) + ReadBuffer.Span[1]);
                conditions.Temperature = new Units.Temperature((float)(((175.72 * temperatureReading) / 65536) - 46.85), Units.Temperature.UnitType.Celsius);

                // humidity
                var humidityReading = (ushort)((ReadBuffer.Span[3] << 8) + ReadBuffer.Span[4]);
                var humidity = (125 * (float)humidityReading / 65536) - 6;
                humidity = Math.Clamp(humidity, 0, 100);
                conditions.Humidity = new RelativeHumidity(humidity, HU.Percent);

                return conditions;
            });
        }
		
        /// <summary>
		/// Turn the heater on or off
        /// </summary>
        /// <param name="heaterOn">Heater status, true = turn heater on, false = turn heater off</param>
        public void Heater(bool heaterOn)
        {
           if(heaterOn)
           {
                Peripheral?.WriteRegister((byte)Commands.HeaterOn, 1);
           }
           else
            {
                Peripheral?.WriteRegister((byte)Commands.HeaterOff, 1);
            }
        }
		
        /// <summary>
        /// Reset the sensor
        /// </summary>
		public void Reset()
        {
            Peripheral?.WriteRegister((byte)Commands.Reset, 1);

            Thread.Sleep(15); //could make this async ...
        }

        private UInt32 GetSerial()
        {
            if (Peripheral == null) return 0;

            var data = new byte[4];

            Peripheral.ReadRegister((byte)Commands.ReadSerial, data);

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