using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using Meadow.Utilities;
using PU = Meadow.Units.Pressure.UnitType;
using TU = Meadow.Units.Temperature.UnitType;
using HU = Meadow.Units.RelativeHumidity.UnitType;

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

        public HeaterProfileType HeaterProfile
        {
            get => heaterProfile;
            set
            {


            }
        }
        HeaterProfileType heaterProfile;

        /// <summary>
        /// Communication bus used to read and write to the BME280 sensor.
        /// </summary>
        /// <remarks>
        /// The BME has both I2C and SPI interfaces. The ICommunicationBus allows the
        /// selection to be made in the constructor.
        /// </remarks>
        readonly Bme680Comms bme680Comms;

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

        /// <summary>
        /// The variable TemperatureFine carries a fine resolution temperature value over to the
        /// pressure compensation formula and could be implemented as a global variable.
        /// </summary>
        protected double TemperatureFine { get; set; }

        /// <summary>
        /// Creates a new instance of the BME680 class
        /// </summary>
        /// <param name="i2cBus">I2C Bus to use for communicating with the sensor</param>
        /// <param name="address">I2C address of the sensor.</param>
        public Bme680(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            configuration = new Configuration();

            bme680Comms = new Bme68xI2C(i2cBus, address);

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
            bme680Comms = new Bme68xSPI(spiBus, chipSelectPort);

            this.configuration = (configuration == null) ? new Configuration() : configuration;

            byte value = bme680Comms.ReadRegister((byte)Registers.STATUS);
            bme680Comms.WriteRegister((byte)Registers.STATUS, value);

            Initialize();
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        protected void Initialize()
        {
            calibration = new Calibration();
            calibration.LoadCalibrationDataFromSensor(bme680Comms);

            // Init the temp and pressure registers
            // Clear the registers so they're in a known state.
            var status = (byte)((((byte)configuration.TemperatureOversample << 5) & 0xe0) |
                                    (((byte)configuration.PressureOversample << 2) & 0x1c));

            bme680Comms.WriteRegister((byte)Registers.CTRL_MEAS, status);

            // Init the humidity registers
            status = (byte)((byte)configuration.HumidityOversample & 0x07);
            bme680Comms.WriteRegister((byte)Registers.CTRL_HUM, status);
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
                var status = bme680Comms.ReadRegister((byte)Registers.CTRL_MEAS);

                // Force a sample
                status = BitHelpers.SetBit(status, 0x00, true);

                bme680Comms.WriteRegister((byte)Registers.CTRL_MEAS, status);
                // Wait for the sample to be taken.
                do
                {
                    status = bme680Comms.ReadRegister((byte)Registers.CTRL_MEAS);
                } while (BitHelpers.GetBitValue(status, 0x00));

                //read temperature
                byte[] data = new byte[3];
                bme680Comms.ReadRegister((byte)Registers.TEMPDATA, data);
                var rawTemperature = (data[0] << 12) | (data[1] << 4) | ((data[2] >> 4) & 0x0);

                //read humidity
                var rawHumidity = bme680Comms.ReadRegisterAsUShort((byte)Registers.HUMIDITYDATA, ByteOrder.BigEndian);

                //read pressure
                bme680Comms.ReadRegister((byte)Registers.PRESSUREDATA, data);
                var rawPressure = (data[0] << 12) | (data[1] << 4) | ((data[2] >> 4) & 0x0);

                //read resistance

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
            double var1 = ((rawTemperature / 16384.0) - (calibration.T1 / 1024.0)) * calibration.T2;
            double var2 = (rawTemperature / 131072.0) - (calibration.T1 / 8192.0);
            var2 *= var2 * calibration.T3 * 16;

            TemperatureFine = var1 + var2;

            double temp = TemperatureFine / 5120.0;
            return new Units.Temperature(temp, TU.Celsius);
        }

        /// <summary>
        /// Compensates the pressure.
        /// </summary>
        /// <param name="adcPressure">The pressure value read from the device.</param>
        /// <returns>The measured pressure.</returns>
        private Pressure CompensatePressure(long adcPressure)
        {
            var var1 = (TemperatureFine / 2.0) - 64000.0;
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
            var temperature = TemperatureFine / 5120.0;
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
    }
}