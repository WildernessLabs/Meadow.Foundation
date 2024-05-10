using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Environmental;
using Meadow.Units;
using Meadow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HU = Meadow.Units.RelativeHumidity.UnitType;
using PU = Meadow.Units.Pressure.UnitType;
using TU = Meadow.Units.Temperature.UnitType;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Represents the Bosch BME68x Temperature, Pressure and Humidity Sensor
    /// </summary>
    public abstract partial class Bme68x :
        PollingSensorBase<(Units.Temperature? Temperature,
                            RelativeHumidity? Humidity,
                            Pressure? Pressure,
                            Resistance? GasResistance)>,
        ITemperatureSensor, IHumiditySensor, IBarometricPressureSensor, IGasResistanceSensor, ISpiPeripheral, II2cPeripheral, ISleepAwarePeripheral, IDisposable
    {
        private event EventHandler<IChangeResult<Units.Temperature>> _temperatureHandlers = default!;
        private event EventHandler<IChangeResult<RelativeHumidity>> _humidityHandlers = default!;
        private event EventHandler<IChangeResult<Pressure>> _pressureHandlers = default!;
        private event EventHandler<IChangeResult<Resistance>> _gasResistanceHandlers = default!;
        private PowerMode _lastRunningPowerMode;

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
        /// Gets / sets the heater profile to be used for measurements
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

                    var profile = busComms.ReadRegister((byte)Registers.CTRL_GAS_1);
                    profile = (byte)((profile & 0xF0) | (byte)value);

                    busComms.WriteRegister((byte)Registers.CTRL_GAS_1, profile);

                    heaterProfile = value;
                }
            }
        }

        private HeaterProfileType heaterProfile;

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

                var filter = busComms.ReadRegister((byte)Registers.CONFIG);
                byte mask = 0x1C;
                filter = (byte)((filter & (byte)~mask) | (byte)value << 2);

                busComms.WriteRegister((byte)Registers.CONFIG, filter);
                filterMode = value;
            }
        }

        private FilteringMode filterMode;

        /// <summary>
        /// Enable / disable the sensor heater
        /// </summary>
        public bool HeaterIsEnabled
        {
            get => heaterIsEnabled;
            set
            {
                var heaterStatus = busComms.ReadRegister((byte)Registers.CTRL_GAS_0);
                var mask = 0x08;
                heaterStatus = (byte)((heaterStatus & (byte)~mask) | Convert.ToByte(!value) << 3);

                busComms.WriteRegister((byte)Registers.CTRL_GAS_0, heaterStatus);
                heaterIsEnabled = value;
            }
        }

        private bool heaterIsEnabled;

        /// <summary>
        /// Enable / disable gas conversions
        /// </summary>
        public bool GasConversionIsEnabled
        {
            get => gasConversionIsEnabled;
            set
            {
                var gasConversion = busComms.ReadRegister((byte)Registers.CTRL_GAS_1);
                byte mask = 0x10;
                gasConversion = (byte)((gasConversion & (byte)~mask) | Convert.ToByte(value) << 5);

                busComms.WriteRegister((byte)Registers.CTRL_GAS_1, gasConversion);
                gasConversionIsEnabled = value;
            }
        }

        private bool gasConversionIsEnabled = false;

        /// <summary>
        /// The default SPI bus speed for the device
        /// </summary>
        public Frequency DefaultSpiBusSpeed => new(10000, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// The SPI bus speed for the device
        /// </summary>
        public Frequency SpiBusSpeed
        {
            get => ((Bme68xSpiCommunications)busComms).BusSpeed;
            set => ((Bme68xSpiCommunications)busComms).BusSpeed = value;
        }

        /// <summary>
        /// The default SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

        /// <summary>
        /// The SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode SpiBusMode
        {
            get => ((Bme68xSpiCommunications)busComms).BusMode;
            set => ((Bme68xSpiCommunications)busComms).BusMode = value;
        }

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Communication bus used to read and write to the BME68x sensor
        /// </summary>
        private readonly IByteCommunications busComms;

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
        public Resistance? GasResistance => Conditions.GasResistance;

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        private readonly bool createdPort = false;
        private readonly Configuration configuration;

        /// <summary>
        /// Calibration data for the sensor
        /// </summary>
        internal Calibration? calibration;

        private double temperatureFine;

        private static readonly byte[] osToMeasCycles = { 0, 1, 2, 4, 8, 16 };
        private static readonly byte[] osToSwitchCount = { 0, 1, 1, 1, 1, 1 };
        private static readonly double[] k1Lookup = { 0.0, 0.0, 0.0, 0.0, 0.0, -1.0, 0.0, -0.8, 0.0, 0.0, -0.2, -0.5, 0.0, -1.0, 0.0, 0.0 };
        private static readonly double[] k2Lookup = { 0.0, 0.0, 0.0, 0.0, 0.1, 0.7, 0.0, -0.8, -0.1, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };

        private readonly List<HeaterProfileConfiguration> heaterConfigs = new();
        private IDigitalOutputPort? chipSelectPort;

        /// <summary>
        /// Creates a new instance of the BME68x class
        /// </summary>
        /// <param name="i2cBus">I2C Bus to use for communicating with the sensor</param>
        /// <param name="address">I2C address</param>
        protected Bme68x(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            configuration = new Configuration();
            busComms = new I2cCommunications(i2cBus, address);

            Initialize();
        }

        /// <summary>
        /// Creates a new instance of the BME68x class
        /// </summary>
        /// <param name="spiBus">The SPI bus connected to the device</param>
        /// <param name="chipSelectPin">The chip select pin</param>
        protected Bme68x(ISpiBus spiBus, IPin chipSelectPin) :
            this(spiBus, chipSelectPin.CreateDigitalOutputPort())
        {
            createdPort = true;
        }

        /// <summary>
        /// Creates a new instance of the BME68x class
        /// </summary>
        /// <param name="spiBus">The SPI bus connected to the device</param>
        /// <param name="chipSelectPort">The chip select pin</param>
        /// <param name="configuration">The BMP68x configuration (optional)</param>
        protected Bme68x(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, Configuration? configuration = null)
        {
            busComms = new Bme68xSpiCommunications(spiBus, this.chipSelectPort = chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);
            this.configuration = configuration ?? new Configuration();

            byte value = busComms.ReadRegister((byte)Registers.STATUS);
            busComms.WriteRegister((byte)Registers.STATUS, value);

            Initialize();
        }

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

        event EventHandler<IChangeResult<Pressure>> ISamplingSensor<Pressure>.Updated
        {
            add => _pressureHandlers += value;
            remove => _pressureHandlers -= value;
        }

        event EventHandler<IChangeResult<Resistance>> ISamplingSensor<Resistance>.Updated
        {
            add => _gasResistanceHandlers += value;
            remove => _gasResistanceHandlers -= value;
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        protected void Initialize()
        {
            Reset();

            calibration = new Calibration();
            calibration.LoadCalibrationDataFromSensor(busComms);

            // Init the temp and pressure registers
            var status = (byte)((((byte)configuration.TemperatureOversample << 5) & 0xe0) |
                                (((byte)configuration.PressureOversample << 2) & 0x1c));

            busComms.WriteRegister((byte)Registers.CTRL_MEAS, status);

            // Init the humidity registers
            status = (byte)((byte)configuration.HumidityOversample & 0x07);
            busComms.WriteRegister((byte)Registers.CTRL_HUM, status);

            //enable gas readings
            GasConversionIsEnabled = true;
        }

        /// <summary>
        /// Perform a complete power-on-reset
        /// </summary>
        public void Reset()
        {
            busComms.WriteRegister((byte)Registers.RESET, 0xB6);
        }

        /// <summary>
        /// Configures a heater profile, making it ready for use.
        /// </summary>
        /// <param name="profile">The heater profile to configure</param>
        /// <param name="targetTemperature">The target temperature (0-400 C)</param>
        /// <param name="duration">The measurement duration (0-4032ms)</param>
        /// <param name="ambientTemperature">The ambient temperature</param>
        public void ConfigureHeatingProfile(HeaterProfileType profile, Units.Temperature targetTemperature, TimeSpan duration, Units.Temperature ambientTemperature)
        {
            // read ambient temperature for resistance calculation
            var heaterResistance = CalculateHeaterResistance(targetTemperature, ambientTemperature);
            var heaterDuration = CalculateHeaterDuration(duration);

            busComms.WriteRegister((byte)(Registers.GAS_WAIT_0 + (byte)profile), heaterDuration);
            busComms.WriteRegister((byte)(Registers.RES_HEAT_0 + (byte)profile), heaterResistance);

            // cache heater configuration
            if (heaterConfigs.Exists(config => config.HeaterProfile == profile))
            {
                heaterConfigs.Remove(heaterConfigs.Single(config => config.HeaterProfile == profile));
            }

            heaterConfigs.Add(new HeaterProfileConfiguration(profile, heaterResistance, duration));
        }

        /// <summary>
        /// Get the current power mode
        /// </summary>
        /// <returns>The power mode</returns>
        public PowerMode GetPowerMode()
        {
            var status = busComms.ReadRegister((byte)Registers.CTRL_MEAS);

            return (PowerMode)(status & 0x03);
        }

        /// <summary>
        /// Sets the power mode to the given mode
        /// </summary>
        /// <param name="powerMode">The <see cref="PowerMode"/> to set.</param>
        public void SetPowerMode(PowerMode powerMode)
        {
            var status = busComms.ReadRegister((byte)Registers.CTRL_MEAS);
            byte mask = 0x03;
            status = (byte)((status & (byte)~mask) | (byte)powerMode);
            busComms.WriteRegister((byte)Registers.CTRL_MEAS, status);
            _lastRunningPowerMode = powerMode;
        }

        /// <summary>
        /// Gets the required time in to perform a measurement. The duration of the gas
        /// measurement is not considered if <see cref="GasConversionIsEnabled"/> is set to false
        /// or the chosen <see cref="HeaterProfile"/> is not configured.
        /// The precision of this duration is within 1ms of the actual measurement time.
        /// </summary>
        /// <param name="profile">The used <see cref="HeaterProfile"/>. </param>
        /// <returns></returns>
        public TimeSpan GetMeasurementDuration(HeaterProfileType profile)
        {
            var measCycles = osToMeasCycles[(int)configuration.TemperatureOversample];
            measCycles += osToMeasCycles[(int)configuration.PressureOversample];
            measCycles += osToMeasCycles[(int)configuration.HumidityOversample];

            var switchCount = osToSwitchCount[(int)configuration.TemperatureOversample];
            switchCount += osToSwitchCount[(int)configuration.PressureOversample];
            switchCount += osToSwitchCount[(int)configuration.HumidityOversample];

            double measDuration = measCycles * 1963;
            measDuration += 477 * switchCount;      // TPH switching duration

            if (GasConversionIsEnabled)
            {
                measDuration += 477 * 5;            // Gas measurement duration
            }

            measDuration += 500;                    // get it to the closest whole number
            measDuration /= 1000.0;                 // convert to ms
            measDuration += 1;                      // wake up duration of 1ms

            if (GasConversionIsEnabled && heaterConfigs.Exists(config => config.HeaterProfile == profile))
            {
                measDuration += heaterConfigs.Single(config => config.HeaterProfile == profile).HeaterDuration.Milliseconds;
            }

            return TimeSpan.FromMilliseconds(Math.Ceiling(measDuration));
        }

        /// <summary>
        /// Raise events for subscribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure, Resistance? GasResistance)> changeResult)
        {
            if (changeResult.New.Temperature is { } temp)
            {
                _temperatureHandlers?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Humidity is { } humidity)
            {
                _humidityHandlers?.Invoke(this, new ChangeResult<RelativeHumidity>(humidity, changeResult.Old?.Humidity));
            }
            if (changeResult.New.Pressure is { } pressure)
            {
                _pressureHandlers?.Invoke(this, new ChangeResult<Pressure>(pressure, changeResult.Old?.Pressure));
            }
            if (changeResult.New.GasResistance is { } gasResistance)
            {
                _gasResistanceHandlers?.Invoke(this, new ChangeResult<Resistance>(gasResistance, changeResult.Old?.GasResistance));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure, Resistance? GasResistance)> ReadSensor()
        {
            configuration.TemperatureOversample = TemperatureOversampleMode;
            configuration.PressureOversample = PressureOversampleMode;
            configuration.HumidityOversample = HumidityOversampleMode;

            return Task.Run(() =>
            {
                (Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure, Resistance? GasResistance) conditions;

                //set onetime measurement
                SetPowerMode(PowerMode.Forced);

                // Read the current control register
                var status = busComms.ReadRegister((byte)Registers.CTRL_MEAS);

                // Force a sample
                status = BitHelpers.SetBit(status, 0x00, true);

                busComms.WriteRegister((byte)Registers.CTRL_MEAS, status);
                // Wait for the sample to be taken.
                do
                {
                    status = busComms.ReadRegister((byte)Registers.CTRL_MEAS);
                } while (BitHelpers.GetBitValue(status, 0x00));

                //read temperature
                byte[] data = new byte[3];
                busComms.ReadRegister((byte)Registers.TEMPDATA, data);
                var rawTemperature = (data[0] << 12) | (data[1] << 4) | ((data[2] >> 4) & 0x0);

                //read humidity
                var rawHumidity = busComms.ReadRegisterAsUShort((byte)Registers.HUMIDITYDATA, ByteOrder.BigEndian);

                //read pressure
                busComms.ReadRegister((byte)Registers.PRESSUREDATA, data);
                var rawPressure = (data[0] << 12) | (data[1] << 4) | ((data[2] >> 4) & 0x0);

                if (GasConversionIsEnabled)
                {
                    Thread.Sleep(GetMeasurementDuration(HeaterProfile));

                    // Read 10 bit gas resistance value from registers
                    var gasResRaw = busComms.ReadRegister((byte)Registers.GAS_RES);
                    var gasRange = busComms.ReadRegister((byte)Registers.GAS_RANGE);
                    var gasRes = (ushort)((ushort)(gasResRaw << 2) + (byte)(gasRange >> 6));
                    gasRange &= 0x0F;
                    conditions.GasResistance = CalculateGasResistance(gasRes, gasRange);
                }
                else
                {
                    conditions.GasResistance = new Resistance(0);
                }

                conditions.Temperature = CompensateTemperature(rawTemperature);
                conditions.Pressure = CompensatePressure(rawPressure);
                conditions.Humidity = CompensateHumidity(rawHumidity);

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
            if (calibration == null) throw new NullReferenceException("Calibration must be defined");

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
            if (calibration == null) throw new NullReferenceException("Calibration must be defined");

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
            if (calibration == null) throw new NullReferenceException("Calibration must be defined");

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

        private Resistance CalculateGasResistance(ushort adcGasRes, byte gasRange)
        {
            if (calibration == null) throw new NullReferenceException("Calibration must be defined");

            var var1 = ((uint)262144) >> gasRange;
            var var2 = (adcGasRes) - 512;
            var2 *= 3;
            var2 = 4096 + var2;
            var gasResistance = 1_000_000.0 * var1 / var2;

            return new Resistance(gasResistance, Resistance.UnitType.Ohms);
        }

        private byte CalculateHeaterResistance(Units.Temperature setTemp, Units.Temperature ambientTemp)
        {
            if (calibration == null) throw new NullReferenceException("Calibration must be defined");

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
        private byte CalculateHeaterDuration(TimeSpan duration)
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

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPort)
                {
                    chipSelectPort?.Dispose();
                }

                IsDisposed = true;
            }
        }

        async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
            => (await Read()).Temperature!.Value;

        async Task<RelativeHumidity> ISensor<RelativeHumidity>.Read()
            => (await Read()).Humidity!.Value;

        async Task<Pressure> ISensor<Pressure>.Read()
            => (await Read()).Pressure!.Value;

        async Task<Resistance> ISensor<Resistance>.Read()
            => (await Read()).GasResistance!.Value;

        /// <inheritdoc/>
        public Task BeforeSleep(CancellationToken cancellationToken)
        {
            SetPowerMode(PowerMode.Sleep);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task AfterWake(CancellationToken cancellationToken)
        {
            SetPowerMode(_lastRunningPowerMode);
            return Task.CompletedTask;
        }
    }
}
