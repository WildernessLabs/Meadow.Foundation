using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;
using HU = Meadow.Units.RelativeHumidity.UnitType;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide access to the Htu21d(f)
    /// temperature and humidity sensors
    /// </summary>
    public partial class Htu21d : Htux1dBase
    {
        /// <summary>
        /// Firmware revision of the sensor.
        /// </summary>
        public byte FirmwareRevision { get; private set; }

        /// <summary>
        /// Create a new Htu21d temperature and humidity sensor
        /// </summary>
        /// <param name="address">Sensor address (default to 0x40)</param>
        /// <param name="i2cBus">I2CBus (default to 100 KHz)</param>
        /// <param name="updateInterval">Update interval, defaults to 1 sec if null</param>
        public Htu21d(II2cBus i2cBus, byte address = (byte)Addresses.Default, TimeSpan? updateInterval = null)
            : base(i2cBus, address, updateInterval)
        {
            Initialize();
        }

        /// <summary>
        /// Initialize HTU21D
        /// </summary>
        protected void Initialize ()
        {
            Peripheral?.Write((byte)Registers.SOFT_RESET);
					 			
			Thread.Sleep(100);

            SetResolution(SensorResolution.TEMP11_HUM11);
        }

        protected override async Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> ReadSensor()
        {
            (Units.Temperature Temperature, RelativeHumidity Humidity) conditions;

            return await Task.Run(() => 
            {
                // humidity
                Peripheral?.Write((byte)Registers.HUMDITY_MEASURE_NOHOLD);
                Thread.Sleep(20); // Maximum conversion time is 12ms (page 5 of the datasheet)
             
                Peripheral?.Read(ReadBuffer.Span[0..2]);// 2 data bytes plus a checksum (we ignore the checksum here)
                var humidityReading = (ushort)((ReadBuffer.Span[0] << 8) + ReadBuffer.Span[1]);
                var humidity = (125 * (float)humidityReading / 65536) - 6;
                humidity = Math.Clamp(humidity, 0, 100);
                conditions.Humidity = new RelativeHumidity(humidity, HU.Percent);

                // temperature
                Peripheral?.Write((byte)Registers.TEMPERATURE_MEASURE_NOHOLD);
                Thread.Sleep(20); // Maximum conversion time is 12ms (page 5 of the datasheet)

                Peripheral?.Read(ReadBuffer.Span[0..2]);// 2 data bytes plus a checksum (we ignore the checksum here)
                var temperatureReading = (short)((ReadBuffer.Span[0] << 8) + ReadBuffer.Span[1]);
                conditions.Temperature = new Units.Temperature((float)(((175.72 * temperatureReading) / 65536) - 46.85), Units.Temperature.UnitType.Celsius);

                return conditions;
            });
        }
		
		/// <summary>
		/// Turn the heater on or off
        /// </summary>
        /// <param name="heaterOn">Heater status, true = turn heater on, false = turn heater off.</param>
        public void Heater(bool heaterOn)
        {
            if (Peripheral == null) return;

            var register = Peripheral.ReadRegister((byte)Registers.READ_HEATER_REGISTER);
            register &= 0xfd;

            if (heaterOn)
            {
                register |= 0x02;
            }
            Peripheral.WriteRegister((byte)Registers.WRITE_HEATER_REGISTER, register);
        }
		
		//Set sensor resolution
        /*******************************************************************************************/
        //Sets the sensor resolution to one of four levels
        //Page 12:
        // 0/0 = 12bit RH, 14bit Temp
        // 0/1 = 8bit RH, 12bit Temp
        // 1/0 = 10bit RH, 13bit Temp
        // 1/1 = 11bit RH, 11bit Temp
        //Power on default is 0/0
        void SetResolution(SensorResolution resolution)
        {
            if (Peripheral == null) return;

            var register = Peripheral.ReadRegister((byte)Registers.READ_USER_REGISTER);

            var res = (byte)resolution;

            register &= 0x73; //Turn off the resolution bits
            res &= 0x81; //Turn off all other bits but resolution bits
            register |= res; //Mask in the requested resolution bits

            //Request a write to user register
            Peripheral.WriteRegister((byte)Registers.WRITE_USER_REGISTER, register); //Write the new resolution bits
        }
    }
}