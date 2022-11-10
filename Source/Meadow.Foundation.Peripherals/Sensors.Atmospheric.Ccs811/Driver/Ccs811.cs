using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide access to the CCS811 C02 and VOC Air Quality Sensor
    /// </summary>
    public partial class Ccs811 :
        ByteCommsSensorBase<(Concentration? Co2, Concentration? Voc)>,
        ICo2Sensor, IVocSensor
    {
        private const int ReadBufferSize = 10;
        private const int WriteBufferSize = 8;

        // internal thread lock
        private byte[] _readingBuffer = new byte[8];

        /// <summary>
        /// Event raised when the CO2 concentration value changes
        /// </summary>
        public event EventHandler<ChangeResult<Concentration>> Co2Updated = delegate { };
        /// <summary>
        /// Event raised when the VOC concentration value changes
        /// </summary>
        public event EventHandler<ChangeResult<Concentration>> VocUpdated = delegate { };

        /// <summary>
        /// The measured CO2 concentration
        /// </summary>
        public Concentration? Co2 => Conditions.Co2;

        /// <summary>
        /// The measured VOC concentration
        /// </summary>
        public Concentration? Voc => Conditions.Voc;

        /// <summary>
        /// Create a new Ccs811 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public Ccs811(II2cBus i2cBus, Addresses address = Addresses.Default)
            : this(i2cBus, (byte)address)
        { }

        /// <summary>
        /// Create a new Ccs811 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public Ccs811(II2cBus i2cBus, byte address)
            : base(i2cBus, address, ReadBufferSize, WriteBufferSize)
        {
            switch (address)
            {
                case 0x5a:
                case 0x5b:
                    // valid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("CCS811 device address must be either 0x5a or 0x5b");
            }

            Initialize();
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        /// <exception cref="Exception">Raised if HW_ID register returns an invalid id</exception>
        protected void Initialize()
        {
            Reset();

            Thread.Sleep(100);

            var id = Peripheral?.ReadRegister((byte)Register.HW_ID);
            if (id != 0x81)
            {
                throw new Exception("Hardware is not identifying as a CCS811");
            }

            Peripheral?.Write((byte)BootloaderCommand.APP_START);

            SetMeasurementMode(MeasurementMode.ConstantPower1s);
            var mode = Peripheral?.ReadRegister((byte)Register.MEAS_MODE);
        }

        private void ShowDebugInfo()
        {
            var ver = Peripheral?.ReadRegister((byte)Register.HW_VERSION);
            Console.WriteLine($"hardware version A = 0x{ver:x2}");

            var fwb = Peripheral?.ReadRegister((byte)Register.FW_BOOT_VERSION);
            Console.WriteLine($"FWB version = 0x{fwb:x4}");

            var fwa = Peripheral?.ReadRegister((byte)Register.FW_APP_VERSION);
            Console.WriteLine($"FWA version = 0x{fwa:x4}");

            // read status
            var status = Peripheral?.ReadRegister((byte)Register.STATUS);
            Console.WriteLine($"status = 0x{status:x2}");
        }

        /// <summary>
        /// Get baseline value
        /// </summary>
        /// <returns>The baseline value</returns>
        public ushort GetBaseline()
        {
            return Peripheral?.ReadRegister((byte)Register.BASELINE) ?? 0;
        }

        /// <summary>
        /// Set the baseline value
        /// </summary>
        /// <param name="value">The new baseline</param>
        public void SetBaseline(ushort value)
        {
            Peripheral?.WriteRegister((byte)Register.BASELINE, (byte)value);
        }

        /// <summary>
        /// Get the current measurement mode
        /// </summary>
        /// <returns>The measurement mode</returns>
        public MeasurementMode GetMeasurementMode()
        {
            return (MeasurementMode)(Peripheral?.ReadRegister((byte)Register.MEAS_MODE) ?? 0);
        }

        /// <summary>
        /// Set the Measurement mode
        /// </summary>
        /// <param name="mode">The new mode</param>
        public void SetMeasurementMode(MeasurementMode mode)
        {
            var m = (byte)mode;
            Peripheral?.WriteRegister((byte)Register.MEAS_MODE, m);
        }

        void Reset()
        {
            Peripheral?.Write(new byte[] { (byte)Register.SW_RESET, 0x11, 0xE5, 0x72, 0x8A });
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override async Task<(Concentration? Co2, Concentration? Voc)> ReadSensor()
        {
            return await Task.Run(() =>
            {
                // data is really in just the first 4, but this gets us status and raw data as well
                Peripheral?.ReadRegister((byte)Register.ALG_RESULT_DATA, _readingBuffer);

                (Concentration co2, Concentration voc) state;
                state.co2 = new Concentration(_readingBuffer[0] << 8 | _readingBuffer[1], Concentration.UnitType.PartsPerMillion);
                state.voc = new Concentration(_readingBuffer[2] << 8 | _readingBuffer[3], Concentration.UnitType.PartsPerBillion);

                return state;
            });
        }

        /// <summary>
        /// Raise events for subcribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Concentration? Co2, Concentration? Voc)> changeResult)
        {
            if (changeResult.New.Co2 is { } co2)
            {
                Co2Updated?.Invoke(this, new ChangeResult<Concentration>(co2, changeResult.Old?.Co2));
            }
            if (changeResult.New.Voc is { } voc)
            {
                VocUpdated?.Invoke(this, new ChangeResult<Concentration>(voc, changeResult.Old?.Voc));
            }

            base.RaiseEventsAndNotify(changeResult);
        }
    }
}