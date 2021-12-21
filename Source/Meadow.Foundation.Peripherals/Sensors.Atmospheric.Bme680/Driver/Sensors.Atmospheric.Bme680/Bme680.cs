using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using PU = Meadow.Units.Pressure.UnitType;
using TU = Meadow.Units.Temperature.UnitType;
using HU = Meadow.Units.RelativeHumidity.UnitType;
using Meadow.Devices;
using System.Buffers;
using Meadow.Utilities;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// BME680 Temperature, Pressure and Humidity Sensor.
    /// </summary>
    /// <remarks>
    /// This class implements the functionality necessary to read the temperature, pressure and humidity
    /// from the Bosch BME680 sensor.
    /// </remarks>
    public partial class Bme680:
        SamplingSensorBase<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure)>,
        ITemperatureSensor, IHumiditySensor, IBarometricPressureSensor
    {
        //==== events
        /// <summary>
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
        public event EventHandler<IChangeResult<Pressure>> PressureUpdated = delegate { };
        public event EventHandler<IChangeResult<RelativeHumidity>> HumidityUpdated = delegate { };

        //==== internals
        protected Memory<byte> readBuffer = new byte[32];
        protected Memory<byte> writeBuffer = new byte[32];

        //==== properties
        public Oversample TemperatureSampleCount { get; set; } = Oversample.OversampleX8;
        public Oversample PressureSampleCount { get; set; } = Oversample.OversampleX8;
        public Oversample HumiditySampleCount { get; set; } = Oversample.OversampleX8;
        private Configuration configuration;

        /// <summary>
        ///     Communication bus used to read and write to the BME280 sensor.
        /// </summary>
        /// <remarks>
        ///     The BME has both I2C and SPI interfaces. The ICommunicationBus allows the
        ///     selection to be made in the constructor.
        /// </remarks>
        private readonly Bme680Comms bme680Comms;

        /// <summary>
        ///     Compensation data from the sensor.
        /// </summary>
        protected TemperatureCompensation temperatureCompensation;
        protected PressureCompensation pressureCompensation;
        protected HumidityCompensation humidityCompensation;


        /// <summary>
        /// The temperature, in degrees celsius (°C), from the last reading.
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The pressure, in hectopascals (hPa), from the last reading. 1 hPa
        /// is equal to one millibar, or 1/10th of a kilopascal (kPa)/centibar.
        /// </summary>
        public Pressure? Pressure => Conditions.Pressure;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public RelativeHumidity? Humidity => Conditions.Humidity;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Meadow.Foundation.Sensors.Barometric.BME680" /> class.
        /// </summary>
        /// <param name="i2c">I2C Bus to use for communicating with the sensor</param>
        /// <param name="address">I2C address of the sensor.</param>
        public Bme680(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
			bme680Comms = new Bme680I2C(i2cBus, address);
            configuration = new Configuration(); // here to avoid the warning
			Initialize();
        }

        public Bme680(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin) :
            this(spiBus, device.CreateDigitalOutputPort(chipSelectPin))
        {
        }

        public Bme680(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, Configuration sensorSettings = null)
        {
            bme680Comms = new Bme680SPI(spiBus, chipSelectPort);
            configuration = new Configuration(); // here to avoid the warning
            //https://github.com/Zanduino/BME680/blob/master/src/Zanshin_BME680.cpp
            bme680Comms.WriteRegister(0x73, bme680Comms.ReadRegister(0x73));

            Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        protected void Initialize()
        {
            // Init the temp and pressure registers
            // Clear the registers so they're in a known state.
            var status = (byte)((((byte)configuration.TemperatureOversample << 5) & 0xe0) |
                                    (((byte)configuration.PressureOversample << 2) & 0x1c));

            bme680Comms.WriteRegister(RegisterAddresses.ControlTemperatureAndPressure.Address, status);

            // Init the humidity registers
            status = (byte)((byte)configuration.HumidityOversample & 0x07);
            bme680Comms.WriteRegister(RegisterAddresses.ControlHumidity.Address, status);
        }

        protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure)> changeResult)
        {
            if (changeResult.New.Temperature is { } temp) {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Humidity is { } humidity) {
                HumidityUpdated?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(humidity, changeResult.Old?.Humidity));
            }
            if (changeResult.New.Pressure is { } pressure) {
                PressureUpdated?.Invoke(this, new ChangeResult<Units.Pressure>(pressure, changeResult.Old?.Pressure));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        protected override async Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure)> ReadSensor()
        {
            Console.WriteLine("F");

            configuration.TemperatureOversample = TemperatureSampleCount;
            configuration.PressureOversample = PressureSampleCount;
            configuration.HumidityOversample = HumiditySampleCount;

            return await Task.Run(() =>
            {
                (Units.Temperature Temperature, RelativeHumidity Humidity, Pressure Pressure) conditions;

                // Read the current control register
                var status = bme680Comms.ReadRegister(RegisterAddresses.ControlTemperatureAndPressure.Address);

                // Force a sample
                status = BitHelpers.SetBit(status, 0x00, true);

                Console.WriteLine("GGG");

                bme680Comms.WriteRegister(RegisterAddresses.ControlTemperatureAndPressure.Address, status);
                // Wait for the sample to be taken.
                do
                {
                    status = bme680Comms.ReadRegister(RegisterAddresses.ControlTemperatureAndPressure.Address);
                } while (BitHelpers.GetBitValue(status, 0x00));

                Console.WriteLine("H");

                var sensorData = readBuffer.Span[0..RegisterAddresses.AllSensors.Length];
                bme680Comms.ReadRegisters(RegisterAddresses.AllSensors.Address, sensorData);

                Console.WriteLine("I");

                var rawPressure = GetRawValue(sensorData.Slice(0, 3));
                var rawTemperature = GetRawValue(sensorData.Slice(3, 3));
                var rawHumidity = GetRawValue(sensorData.Slice(6, 2));
                //var rawVoc = GetRawValue(sensorData.Slice(8, 2));

                Console.WriteLine("J");

                bme680Comms.ReadRegisters(RegisterAddresses.CompensationData1.Address, readBuffer.Span[0..RegisterAddresses.CompensationData1.Length]);
                var compensationData1 = readBuffer.Span[0..RegisterAddresses.CompensationData1.Length].ToArray();

                bme680Comms.ReadRegisters(RegisterAddresses.CompensationData2.Address, readBuffer.Span[0..RegisterAddresses.CompensationData2.Length]);
                var compensationData2 = readBuffer.Span[0..RegisterAddresses.CompensationData2.Length].ToArray();

                Console.WriteLine("K");

                var compensationData = ArrayPool<byte>.Shared.Rent(64);
                try
                {
                    Array.Copy(compensationData1, 0, compensationData, 0, compensationData1.Length);
                    Array.Copy(compensationData2, 0, compensationData, 25, compensationData2.Length);

                    var temp = RawToTemperature(rawTemperature,
                        new TemperatureCompensation(compensationData));

                    var pressure = RawToPressure(temp, rawPressure,
                        new PressureCompensation(compensationData));
                    var humidity = RawToHumidity(temp, rawHumidity,
                        new HumidityCompensation(compensationData));

                    conditions.Temperature = new Units.Temperature(temp, TU.Celsius);
                    conditions.Pressure = new Pressure(pressure, PU.Pascal);
                    conditions.Humidity = new RelativeHumidity(humidity, HU.Percent);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(compensationData, true);
                }

                Console.WriteLine("Return conditions");
                return conditions;
            });
        }

        private static int GetRawValue(Span<byte> data)
        {
            if (data.Length == 3)
            {
                return (data[0] << 12) | (data[1] << 4) | ((data[2] >> 4) & 0x0);
            }
            if (data.Length == 2)
            {
                return (data[0] << 8) | data[1];
            }
            return 0;
        }

        //In celcius
        private static double RawToTemperature(int adcTemperature, TemperatureCompensation temperatureCompensation)
        {
            var var1 = ((adcTemperature / 16384.0) - (temperatureCompensation.T1 / 1024.0)) *
                       temperatureCompensation.T2;
            var var2 = ((adcTemperature / 131072) - (temperatureCompensation.T1 / 8192.0));
            var var3 = var2 * ((adcTemperature / 131072.0) - (temperatureCompensation.T1 / 8192.0));
            var var4 = var3 * (temperatureCompensation.T3 * 16.0);
            var tFine = var1 + var4;
            return tFine / 5120.0;
        }

        private static double RawToPressure(double temperature, int adcPressure, PressureCompensation pressureCompensation)
        {
            double var1;
            double var2;
            double var3;
            double calc_pres;

            var PC = pressureCompensation;

            var tFine = temperature * 5120;

            var1 = ((tFine / 2.0) - 64000.0);
            var2 = var1 * var1 * ((PC.P6) / (131072.0));
            var2 = var2 + (var1 * (PC.P5) * 2.0);
            var2 = (var2 / 4.0) + ((PC.P4) * 65536.0);
            var1 = ((((PC.P3 * var1 * var1) / 16384.0) + (PC.P2 * var1)) / 524288.0);
            var1 = ((1.0f + (var1 / 32768.0)) * (PC.P1));
            calc_pres = (1048576.0 - ((float)adcPressure));

            /* Avoid exception caused by division by zero */
            if ((int)var1 != 0)
            {
                calc_pres = (((calc_pres - (var2 / 4096.0)) * 6250.0) / var1);
                var1 = ((PC.P9) * calc_pres * calc_pres) / 2147483648.0f;
                var2 = calc_pres * ((PC.P8) / 32768.0);
                var3 = ((calc_pres / 256.0) * (calc_pres / 256.0) * (calc_pres / 256.0) * (PC.P10 / 131072.0));
                calc_pres = (calc_pres + (var1 + var2 + var3 + (PC.P7 * 128.0)) / 16.0);
            }
            else
            {
                calc_pres = 0;
            }

            return calc_pres;
        }

        private static double RawToHumidity(double temp, int adcHumidity, HumidityCompensation humidityCompensation)
        {
            var var1 = adcHumidity - ((humidityCompensation.H1 * 16.0) + ((humidityCompensation.H3 / 2.0) * temp));
            var var2 = var1 * ((humidityCompensation.H2 / 262144.0) * (1.0 + ((humidityCompensation.H4 / 16384.0) * temp) + ((humidityCompensation.H5 / 1048576.0) * temp * temp)));
            var var3 = humidityCompensation.H6 / 16384.0;
            var var4 = humidityCompensation.H7 / 2097152.0;
            return var2 + ((var3 + (var4 * temp)) * var2 * var2);
        }

        private byte ReadRegister(Register register)
        {
            return bme680Comms.ReadRegister(register.Address);
        }

        private Span<byte> ReadRegisters(Register register)
        {
            bme680Comms.ReadRegisters(register.Address, readBuffer.Span[0..register.Length]);
            return readBuffer.Slice(0, register.Length).Span;
        }

    }
}
