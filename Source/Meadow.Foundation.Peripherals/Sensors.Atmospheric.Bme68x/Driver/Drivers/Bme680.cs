using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using Meadow.Utilities;
using PU = Meadow.Units.Pressure.UnitType;
using TU = Meadow.Units.Temperature.UnitType;
using HU = Meadow.Units.RelativeHumidity.UnitType;
using System.Collections.Generic;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Represents the Bosch BME680 Temperature, Pressure and Humidity Sensor.
    /// </summary>
    public partial class Bme680:
        SamplingSensorBase<(Units.Temperature? Temperature, 
                            RelativeHumidity? Humidity, 
                            Pressure? Pressure, 
                            Resistance? GasResistance)>,
        ITemperatureSensor, IHumiditySensor, IBarometricPressureSensor
    {
        /// <summary>
        /// Raised when the temperature value changes
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
       
        /// <summary>
        /// Raised when the pressure value changes
        /// </summary>
        public event EventHandler<IChangeResult<Pressure>> PressureUpdated = delegate { };
       
        /// <summary>
        /// Raised when the humidity value changes
        /// </summary>
        public event EventHandler<IChangeResult<RelativeHumidity>> HumidityUpdated = delegate { };

        /// <summary>
        /// Raised when the gas resistance value changes
        /// </summary>
        public event EventHandler<IChangeResult<Resistance>> GasResistanceUpdated = delegate { };

        /// <summary>
        /// The temperature oversampling mode
        /// </summary>
        public Oversample TemperatureOversampleMode
        {
            get => configuration.TemperatureOversample;
            set => configuration.TemperatureOversample = value;
        }
       
        /// <summary>
        /// The pressure oversampling mode
        /// </summary>
        public Oversample PressureOversampleMode
        {
            get => configuration.PressureOversample;
            set => configuration.PressureOversample = value;
        }

        /// <summary>
        /// The humidity oversampling mode
        /// </summary>
        public Oversample HumidityOversampleMode
        {
            get => configuration.HumidityOversample;
            set => configuration.HumidityOversample = value;
        }

        /// <summary>
        /// Gets or sets the heater profile to be used for measurements.
        /// Current heater profile is only set if the chosen profile is configured.
        /// </summary>
        public HeaterProfileType HeaterProfile
        {
            get => heaterProfile;
            set
            {
                if (heaterConfigs.Exists(config => config.HeaterProfile == value))
                {
                    if (!Enum.IsDefined(typeof(HeaterProfileType), value))
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }

                    var profile = sensor.ReadRegister((byte)Registers.CTRL_GAS_1);
                    profile = (byte)((profile & 0x0F) | (byte)value);

                    sensor.WriteRegister((byte)Registers.CTRL_GAS_1, profile);

                    heaterProfile = value;
                }
            }
        }
        HeaterProfileType heaterProfile;

        /// <summary>
        /// Gets / sets the filtering mode to be used for measurements
        /// </summary>
        public FilteringMode FilterMode
        {
            get => filterMode;
            set
            {
                if (!Enum.IsDefined(typeof(FilteringMode), value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                var filter = sensor.ReadRegister((byte)Registers.CONFIG);
                byte mask = 0x1C;
                filter = (byte)((filter & (byte)~mask) | (byte)value << 2);

                sensor.WriteRegister((byte)Registers.CONFIG, filter);
                filterMode = value;
            }
        }
        FilteringMode filterMode;

        /// <summary>
        /// Enable / disable the sensor heater
        /// </summary>
        public bool HeaterIsEnabled
        {
            get => heaterIsEnabled;
            set
            {
                var heaterStatus = sensor.ReadRegister((byte)Registers.CTRL_GAS_0);
                var mask = 0x08;
                heaterStatus = (byte)((heaterStatus & (byte)~mask) | Convert.ToByte(!value) << 3);

                sensor.WriteRegister((byte)Registers.CTRL_GAS_0, heaterStatus);
                heaterIsEnabled = value;
            }
        }
        bool heaterIsEnabled;

        /// <summary>
        /// Enable / disable gas conversions
        /// </summary>
        public bool GasConversionIsEnabled
        {
            get => gasConversionIsEnabled;
            set
            {
                var gasConversion = sensor.ReadRegister((byte)Registers.CTRL_GAS_1);
                byte mask = 0x10;
                gasConversion = (byte)((gasConversion & (byte)~mask) | Convert.ToByte(value) << 4);

                sensor.WriteRegister((byte)Registers.CTRL_GAS_1, gasConversion);
                gasConversionIsEnabled = value;
            }
        }
        bool gasConversionIsEnabled;

        /// <summary>
        /// Communication bus used to read and write to the BME280 sensor.
        /// </summary>
        /// <remarks>
        /// The BME has both I2C and SPI interfaces. The ICommunicationBus allows the
        /// selection to be made in the constructor.
        /// </remarks>
        readonly Bme68xComms sensor;

        /// <summary>
        /// The current temperature
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The current pressure
        /// </summary>
        public Pressure? Pressure => Conditions.Pressure;

        /// <summary>
        /// The current humidity, in percent relative humidity
        /// </summary>
        public RelativeHumidity? Humidity => Conditions.Humidity;

        /// <summary>
        /// The current gas resistance
        /// </summary>
        public Resistance? GetResistance => Conditions.GasResistance;

        readonly Memory<byte> readBuffer = new byte[32];
        readonly Memory<byte> writeBuffer = new byte[32];

        readonly Configuration configuration;

        /// <summary>
        /// Calibration data for the sensor
        /// </summary>
        internal Calibration calibration;

        private double temperatureFine;

        private static readonly byte[] s_osToMeasCycles = { 0, 1, 2, 4, 8, 16 };
        private static readonly byte[] s_osToSwitchCount = { 0, 1, 1, 1, 1, 1 };
        private static readonly double[] s_k1Lookup = { 0.0, 0.0, 0.0, 0.0, 0.0, -1.0, 0.0, -0.8, 0.0, 0.0, -0.2, -0.5, 0.0, -1.0, 0.0, 0.0 };
        private static readonly double[] s_k2Lookup = { 0.0, 0.0, 0.0, 0.0, 0.1, 0.7, 0.0, -0.8, -0.1, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };

        private readonly List<HeaterProfileConfiguration> heaterConfigs = new List<HeaterProfileConfiguration>();

        /// <summary>
        /// Creates a new instance of the BME680 class
        /// </summary>
        /// <param name="i2cBus">I2C Bus to use for communicating with the sensor</param>
        /// <param name="address">I2C address of the sensor.</param>
        public Bme680(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            configuration = new Configuration();

            sensor = new Bme68xI2C(i2cBus, address);

            Initialize();
        }

        /// <summary>
        /// Creates a new instance of the BME680 class
        /// </summary>
        /// <param name="device">The Meadow device to create the chip select port</param>
        /// <param name="spiBus">The SPI bus connected to the device</param>
        /// <param name="chipSelectPin">The chip select pin</param>
        public Bme680(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin) :
            this(spiBus, device.CreateDigitalOutputPort(chipSelectPin))
        {
        }

        /// <summary>
        /// Creates a new instance of the BME680 class
        /// </summary>
        /// <param name="spiBus">The SPI bus connected to the device</param>
        /// <param name="chipSelectPort">The chip select pin</param>
        /// <param name="configuration">The BMP680 configuration (optional)</param>
        public Bme680(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, Configuration? configuration = null)
        {
            sensor = new Bme68xSPI(spiBus, chipSelectPort);

            this.configuration = (configuration == null) ? new Configuration() : configuration;

            byte value = sensor.ReadRegister((byte)Registers.STATUS);
            sensor.WriteRegister((byte)Registers.STATUS, value);

            Initialize();
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        protected void Initialize()
        {
            calibration = new Calibration();
            calibration.LoadCalibrationDataFromSensor(sensor);

            // Init the temp and pressure registers
            // Clear the registers so they're in a known state.
            var status = (byte)((((byte)configuration.TemperatureOversample << 5) & 0xe0) |
                                    (((byte)configuration.PressureOversample << 2) & 0x1c));

            sensor.WriteRegister((byte)Registers.CTRL_MEAS, status);

            // Init the humidity registers
            status = (byte)((byte)configuration.HumidityOversample & 0x07);
            sensor.WriteRegister((byte)Registers.CTRL_HUM, status);
        }

        protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure, Resistance? GasResistance)> changeResult)
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
            if (changeResult.New.GasResistance is { } gasResistance)
            {
                GasResistanceUpdated?.Invoke(this, new ChangeResult<Units.Resistance>(gasResistance, changeResult.Old?.GasResistance));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        protected override async Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure, Resistance? GasResistance)> ReadSensor()
        {
            configuration.TemperatureOversample = TemperatureOversampleMode;
            configuration.PressureOversample = PressureOversampleMode;
            configuration.HumidityOversample = HumidityOversampleMode;

            return await Task.Run(() =>
            {
                (Units.Temperature Temperature, RelativeHumidity Humidity, Pressure Pressure, Resistance GasResistance) conditions;

                // Read the current control register
                var status = sensor.ReadRegister((byte)Registers.CTRL_MEAS);

                // Force a sample
                status = BitHelpers.SetBit(status, 0x00, true);

                sensor.WriteRegister((byte)Registers.CTRL_MEAS, status);
                // Wait for the sample to be taken.
                do
                {
                    status = sensor.ReadRegister((byte)Registers.CTRL_MEAS);
                } while (BitHelpers.GetBitValue(status, 0x00));

                //read temperature
                byte[] data = new byte[3];
                sensor.ReadRegister((byte)Registers.TEMPDATA, data);
                var rawTemperature = (data[0] << 12) | (data[1] << 4) | ((data[2] >> 4) & 0x0);

                //read humidity
                var rawHumidity = sensor.ReadRegisterAsUShort((byte)Registers.HUMIDITYDATA, ByteOrder.BigEndian);

                //read pressure
                sensor.ReadRegister((byte)Registers.PRESSUREDATA, data);
                var rawPressure = (data[0] << 12) | (data[1] << 4) | ((data[2] >> 4) & 0x0);

                // Read 10 bit gas resistance value from registers
                var gasResRaw = sensor.ReadRegister((byte)Registers.GAS_RES);
                var gasRange = sensor.ReadRegister((byte)Registers.GAS_RANGE);
                var gasRes = (ushort)((ushort)(gasResRaw << 2) + (byte)(gasRange >> 6));
                gasRange &= 0x0F;

                conditions.Temperature = CompensateTemperature(rawTemperature);
                conditions.Pressure = CompensatePressure(rawPressure);
                conditions.Humidity = CompensateHumidity(rawHumidity);
                conditions.GasResistance = CalculateGasResistance(gasRes, gasRange);

                return conditions;
            });
        }

        /// <summary>
        /// Compensates the temperature
        /// </summary>
        /// <param name="rawTemperature">The temperature value read from the device</param>
        /// <returns>The temperature</returns>
        protected Units.Temperature CompensateTemperature(int rawTemperature)
        {
            double var1 = ((rawTemperature / 16384.0) - (calibration.T1 / 1024.0)) * calibration.T2;
            double var2 = (rawTemperature / 131072.0) - (calibration.T1 / 8192.0);
            var2 *= var2 * calibration.T3 * 16;

            temperatureFine = var1 + var2;

            double temp = temperatureFine / 5120.0;
            return new Units.Temperature(temp, TU.Celsius);
        }

        /// <summary>
        /// Compensates the pressure.
        /// </summary>
        /// <param name="adcPressure">The pressure value read from the device.</param>
        /// <returns>The measured pressure.</returns>
        private Pressure CompensatePressure(long adcPressure)
        {
            var var1 = (temperatureFine / 2.0) - 64000.0;
            var var2 = var1 * var1 * (calibration.P6 / 131072.0);
            var2 += (var1 * calibration.P5 * 2.0);
            var2 = (var2 / 4.0) + (calibration.P4 * 65536.0);
            var1 = ((calibration.P3 * var1 * var1 / 16384.0) + (calibration.P2 * var1)) / 524288.0;
            var1 = (1.0 + (var1 / 32768.0)) * calibration.P1;
            var pressure = 1048576.0 - adcPressure;

            // Avoid divide by zero exception
            if (var1 != 0)
            {
                pressure = (pressure - (var2 / 4096.0)) * 6250.0 / var1;
                var1 = calibration.P9 * pressure * pressure / 2147483648.0;
                var2 = pressure * (calibration.P8 / 32768.0);
                var var3 = (pressure / 256.0) * (pressure / 256.0) * (pressure / 256.0)
                    * (calibration.P10 / 131072.0);
                pressure += (var1 + var2 + var3 + (calibration.P7 * 128.0)) / 16.0;
            }
            else
            {
                pressure = 0;
            }

            return new Pressure(pressure, PU.Pascal);
        }

        /// <summary>
        /// Compensates the humidity.
        /// </summary>
        /// <param name="adcHumidity">The humidity value read from the device.</param>
        /// <returns>The percentage relative humidity.</returns>
        private RelativeHumidity CompensateHumidity(int adcHumidity)
        {
            var temperature = temperatureFine / 5120.0;
            var var1 = adcHumidity - ((calibration.H1 * 16.0) + ((calibration.H3 / 2.0) * temperature));
            var var2 = var1 * ((calibration.H2 / 262144.0) * (1.0 + ((calibration.H4 / 16384.0) * temperature)
                + ((calibration.H5 / 1048576.0) * temperature * temperature)));
            var var3 = calibration.H6 / 16384.0;
            var var4 = calibration.H7 / 2097152.0;
            var humidity = var2 + ((var3 + (var4 * temperature)) * var2 * var2);

            humidity = Math.Max(humidity, 0);
            humidity = Math.Min(humidity, 100.0);

            return new RelativeHumidity(humidity, HU.Percent);
        }

        Resistance CalculateGasResistance(ushort adcGasRes, byte gasRange)
        {
            if (calibration is null)
            {
                throw new Exception($"{nameof(Bme680)} is incorrectly configured.");
            }

            var var1 = 1340.0 + 5.0 * calibration.RangeSwErr;
            var var2 = var1 * (1.0 + s_k1Lookup[gasRange] / 100.0);
            var var3 = 1.0 + s_k2Lookup[gasRange] / 100.0;
            var gasResistance = 1.0 / (var3 * 0.000000125 * (1 << gasRange) * ((adcGasRes - 512.0) / var2 + 1.0));

            return new Resistance(gasResistance, Resistance.UnitType.Ohms);
        }

        byte CalculateHeaterResistance(Units.Temperature setTemp, Units.Temperature ambientTemp)
        {
            if (calibration is null)
            {
                throw new Exception($"{nameof(Bme680)} is incorrectly configured.");
            }

            // limit maximum temperature to 400°C
            double temp = setTemp.Celsius;
            if (temp > 400)
            {
                temp = 400;
            }

            var var1 = calibration.Gh1 / 16.0 + 49.0;
            var var2 = calibration.Gh2 / 32768.0 * 0.0005 + 0.00235;
            var var3 = calibration.Gh3 / 1024.0;
            var var4 = var1 * (1.0 + var2 * temp);
            var var5 = var4 + var3 * ambientTemp.Celsius;
            var heaterResistance = (byte)(3.4 * (var5 * (4.0 / (4.0 + calibration.ResHeatRange)) * (1.0 / (1.0 + calibration.ResHeatVal * 0.002)) - 25));

            return heaterResistance;
        }

        // The duration is interpreted as follows:
        // Byte [7:6]: multiplication factor of 1, 4, 16 or 64
        // Byte [5:0]: 64 timer values, 1ms step size, rounded down
        byte CalculateHeaterDuration(TimeSpan duration)
        {
            byte factor = 0;
            byte durationValue;

            ushort shortDuration = (ushort)duration.Milliseconds;
            // check if value exceeds maximum duration
            if (shortDuration > 0xFC0)
            {
                durationValue = 0xFF;
            }
            else
            {
                while (shortDuration > 0x3F)
                {
                    shortDuration = (ushort)(shortDuration >> 2);
                    factor += 1;
                }
                durationValue = (byte)(shortDuration + factor * 64);
            }
            return durationValue;
        }
    }
}