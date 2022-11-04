using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using HU = Meadow.Units.RelativeHumidity.UnitType;
using TU = Meadow.Units.Temperature.UnitType;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide access to the Si70xx series (Si7020, Si7021, and Si7030)
    /// temperature and humidity sensors.
    /// </summary>
    public partial class Si70xx :
        ByteCommsSensorBase<(Units.Temperature? Temperature, RelativeHumidity? Humidity)>,
        ITemperatureSensor, IHumiditySensor
    {
        /// <summary>
        /// Raised when the temperature value changes
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
        
        /// <summary>
        /// Raised when the humidity value changes
        /// </summary>
        public event EventHandler<IChangeResult<RelativeHumidity>> HumidityUpdated = delegate { };

        /// <summary>
        /// Default SPI bus speed
        /// </summary>
        public static Frequency DEFAULT_SPEED = new Frequency(400, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// The temperature, from the last reading
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading
        /// </summary>
        public RelativeHumidity? Humidity => Conditions.Humidity;

        /// <summary>
        /// Serial number of the device
        /// </summary>
        public ulong SerialNumber { get; private set; }

        /// <summary>
        /// Device type as extracted from the serial number
        /// </summary>
        public DeviceType SensorType { get; private set; }

        /// <summary>
        /// Firmware revision of the sensor
        /// </summary>
        public byte FirmwareRevision { get; private set; }

        /// <summary>
        /// Create a new SI7021 temperature and humidity sensor
        /// </summary>
        /// <param name="i2cBus">I2CBus</param>
        /// <param name="address">I2C address (default to 0x40)</param>
        public Si70xx(II2cBus i2cBus, byte address = (byte)Address.Default)
            : base(i2cBus, address, 8, 3)
        {
            Initialize();
        }
        /// <summary>
        /// Reset the sensor
        /// </summary>
        protected void Reset()
        {
            Peripheral?.Write(CMD_RESET);
            Thread.Sleep(100);
        }

        /// <summary>
        /// Initalize the sensor
        /// </summary>
        protected void Initialize()
        {
            Span<byte> tx = WriteBuffer.Span[0..2];

            Reset();

            SerialNumber = 0;

            tx[0] = READ_ID_PART1;
            tx[1] = READ_ID_PART2;
            Peripheral?.Exchange(tx, ReadBuffer.Span);
            for (var index = 0; index < 4; index++)
            {
                SerialNumber <<= 8;
                SerialNumber += ReadBuffer.Span[index * 2];
            }

            tx[0] = READ_2ND_ID_PART1;
            tx[1] = READ_2ND_ID_PART2;
            Peripheral?.Exchange(tx, ReadBuffer.Span);

            SerialNumber <<= 8;
            SerialNumber += ReadBuffer.Span[0];
            SerialNumber <<= 8;
            SerialNumber += ReadBuffer.Span[1];
            SerialNumber <<= 8;
            SerialNumber += ReadBuffer.Span[3];
            SerialNumber <<= 8;
            SerialNumber += ReadBuffer.Span[4];
            if ((ReadBuffer.Span[0] == 0) || (ReadBuffer.Span[0] == 0xff))
            {
                SensorType = DeviceType.EngineeringSample;
            }
            else
            {
                SensorType = (DeviceType)ReadBuffer.Span[0];
            }

            SetResolution(SensorResolution.TEMP11_HUM11);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected async override Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> ReadSensor()
        {
            (Units.Temperature Temperature, RelativeHumidity Humidity) conditions;

            return await Task.Run(() =>
            {
                Peripheral?.Write(HUMDITY_MEASURE_NOHOLD);
                Thread.Sleep(25); // Maximum conversion time is 12ms (page 5 of the datasheet).
                Peripheral?.Read(ReadBuffer.Span); // 2 data bytes plus a checksum (we ignore the checksum here)
                var humidityReading = (ushort)((ReadBuffer.Span[0] << 8) + ReadBuffer.Span[1]);
                conditions.Humidity = new RelativeHumidity(((125 * (float)humidityReading) / 65536) - 6, HU.Percent);
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

                Peripheral?.Write(TEMPERATURE_MEASURE_NOHOLD);
                Thread.Sleep(25); // Maximum conversion time is 12ms (page 5 of the datasheet).
                Peripheral?.Read(ReadBuffer.Span); // 2 data bytes plus a checksum (we ignore the checksum here)
                var temperatureReading = (short)((ReadBuffer.Span[0] << 8) + ReadBuffer.Span[1]);
                conditions.Temperature = new Units.Temperature((float)(((175.72 * temperatureReading) / 65536) - 46.85), TU.Celsius);

                return conditions;
            });
        }

        /// <summary>
        /// Inheritance-safe way to raise events and notify observers.
        /// </summary>
        /// <param name="changeResult"></param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> changeResult)
        {
            if (changeResult.New.Temperature is { } temp)
            {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Humidity is { } humidity)
            {
                HumidityUpdated?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(humidity, changeResult.Old?.Humidity));
            }

            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Turn the heater on or off
        /// </summary>
        /// <param name="onOrOff">Heater status, true = turn heater on, false = turn heater off.</param>
        public void Heater(bool onOrOff)
        {
            var register = Peripheral?.ReadRegister((byte)Register.USER_REG_1) ?? 0;
            register &= 0xfd;

            if (onOrOff)
            {
                register |= 0x02;
            }
            Peripheral?.WriteRegister((byte)Register.USER_REG_1, register);
        }

        /// <summary>
        /// Sets the sensor resolution to one of four levels
        /// Page 12:
        /// 0/0 = 12bit RH, 14bit Temp
        /// 0/1 = 8bit RH, 12bit Temp
        /// 1/0 = 10bit RH, 13bit Temp
        /// 1/1 = 11bit RH, 11bit Temp
        ///Power on default is 0/0
        /// </summary>
        /// <param name="resolution">The resolution to set</param>
        void SetResolution(SensorResolution resolution)
        {
            var register = Peripheral?.ReadRegister((byte)Register.USER_REG_1) ?? 0;

            var res = (byte)resolution;

            register &= 0x73; //Turn off the resolution bits
            res &= 0x81; //Turn off all other bits but resolution bits
            register |= res; //Mask in the requested resolution bits

            //Request a write to user register
            Peripheral?.WriteRegister((byte)Register.USER_REG_1, register); //Write the new resolution bits
        }
    }
}