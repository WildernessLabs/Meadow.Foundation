using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;
using TU = Meadow.Units.Temperature.UnitType;
using HU = Meadow.Units.RelativeHumidity.UnitType;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide access to the Htu21d(f)
    /// temperature and humidity sensors
    /// </summary>
    public partial class Htu21d :
        ByteCommsSensorBase<(Units.Temperature? Temperature, RelativeHumidity? Humidity)>,
        ITemperatureSensor, IHumiditySensor
    {
        //==== Events
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
        public event EventHandler<IChangeResult<RelativeHumidity>> HumidityUpdated = delegate { };

        //==== internals

        //==== propertires		
        public int DEFAULT_SPEED => 400;

        /// <summary>
        /// The temperature, from the last reading.
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public RelativeHumidity? Humidity => Conditions.Humidity;

        /// <summary>
        /// Serial number of the device.
        /// </summary>
        public ulong SerialNumber { get; private set; }

        /// <summary>
        /// Firmware revision of the sensor.
        /// </summary>
        public byte FirmwareRevision { get; private set; }
		
        /// <summary>
        /// Create a new Htu21d temperature and humidity sensor.
        /// </summary>
        /// <param name="address">Sensor address (default to 0x40).</param>
        /// <param name="i2cBus">I2CBus (default to 100 KHz).</param>
        public Htu21d(II2cBus i2cBus, byte address = 0x40, int updateIntervalMs = 1000)
            : base(i2cBus, address, updateIntervalMs)
        {
            Initialize();
        }

        protected void Initialize ()
        {
            Peripheral.Write(SOFT_RESET);
					 			
			Thread.Sleep(100);

            SetResolution(SensorResolution.TEMP11_HUM11);
        }

        protected override async Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> ReadSensor()
        {
            (Units.Temperature Temperature, RelativeHumidity Humidity) conditions;

            return await Task.Run(() => {
                // ---- HUMIDITY
                //Bus.WriteBytes(HUMDITY_MEASURE_NOHOLD);
                Peripheral.Write(HUMDITY_MEASURE_NOHOLD);
                Thread.Sleep(25); // Maximum conversion time is 12ms (page 5 of the datasheet).
                //Bus.ReadBytes(_rx, 3); // 2 data bytes plus a checksum (we ignore the checksum here)
                Peripheral.Read(ReadBuffer.Span[0..3]);// 2 data bytes plus a checksum (we ignore the checksum here)
                var humidityReading = (ushort)((ReadBuffer.Span[0] << 8) + ReadBuffer.Span[1]);
                conditions.Humidity = new RelativeHumidity(((125 * (float)humidityReading) / 65536) - 6, RelativeHumidity.UnitType.Percent);
                if (conditions.Humidity < new RelativeHumidity(0, HU.Percent))
                {
                    conditions.Humidity = new RelativeHumidity(0, HU.Percent);
                }
                else
                {
                    if (conditions.Humidity > new RelativeHumidity(100, HU.Percent))
                    {
                        conditions.Humidity = new RelativeHumidity(100, HU.Percent);
                    }
                }

                // ---- TEMPERATURE
                //Bus.WriteBytes(TEMPERATURE_MEASURE_NOHOLD);
                Peripheral.Write(TEMPERATURE_MEASURE_NOHOLD);
                Thread.Sleep(25); // Maximum conversion time is 12ms (page 5 of the datasheet).
                //Bus.ReadBytes(_rx, 3); // 2 data bytes plus a checksum (we ignore the checksum here)
                Peripheral.Read(ReadBuffer.Span[0..3]);// 2 data bytes plus a checksum (we ignore the checksum here)
                var temperatureReading = (short)((ReadBuffer.Span[0] << 8) + ReadBuffer.Span[1]);
                conditions.Temperature = new Units.Temperature((float)(((175.72 * temperatureReading) / 65536) - 46.85), Units.Temperature.UnitType.Celsius);

                return conditions;
            });
        }
		
        /// <summary>
        /// Inheritance-safe way to raise events and notify observers.
        /// </summary>
        /// <param name="changeResult"></param>
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
		/// Turn the heater on or off.
        /// </summary>
        /// <param name="onOrOff">Heater status, true = turn heater on, false = turn heater off.</param>
        public void Heater(bool onOrOff)
        {
            //var register = Bus.ReadRegisterByte(READ_HEATER_REGISTER);
            var register = Peripheral.ReadRegister(READ_HEATER_REGISTER);
            register &= 0xfd;

            if (onOrOff)
            {
                register |= 0x02;
            }
            //Bus.WriteRegister(WRITE_HEATER_REGISTER, register);
            Peripheral.WriteRegister(WRITE_HEATER_REGISTER, register);
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
            var register = Peripheral.ReadRegister(READ_USER_REGISTER);

            //userRegister &= 0b01111110; //Turn off the resolution bits
            //resolution &= 0b10000001; //Turn off all other bits but resolution bits
            //userRegister |= resolution; //Mask in the requested resolution bits

            var res = (byte)resolution;

            register &= 0x73; //Turn off the resolution bits
            res &= 0x81; //Turn off all other bits but resolution bits
            register |= res; //Mask in the requested resolution bits

            //Request a write to user register
            Peripheral.WriteRegister(WRITE_USER_REGISTER, register); //Write the new resolution bits
        }
    }
}