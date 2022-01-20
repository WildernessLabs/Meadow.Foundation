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

        public Ccs811(II2cBus i2cBus, Addresses address = Addresses.Default)
            : this(i2cBus, (byte)address)
        {
        }

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

            Init();
        }

        protected void Init()
        {
            // reset
            Reset();

            // wait for the chip to do its thing
            Thread.Sleep(100);

            // read chip ID to make sure it's a CCS
            //var id = Bus.ReadRegisterByte((byte)Register.HW_ID);
            var id = Peripheral.ReadRegister((byte)Register.HW_ID);
            if (id != 0x81)
            {
                throw new Exception("Hardware is not identifying as a CCS811");
            }

            // start the firmware app
            //Bus.WriteBytes((byte)BootloaderCommand.APP_START);
            Peripheral.Write((byte)BootloaderCommand.APP_START);

            // change mode
            SetMeasurementMode(MeasurementMode.ConstantPower1s);
            //var mode = Bus.ReadRegisterByte((byte)Register.MEAS_MODE);
            var mode = Peripheral.ReadRegister((byte)Register.MEAS_MODE);
        }

        private void ShowDebugInfo()
        {
            //var ver = Bus.ReadRegisterByte((byte)Register.HW_VERSION);
            var ver = Peripheral.ReadRegister((byte)Register.HW_VERSION);
            Console.WriteLine($"hardware version A = 0x{ver:x2}");

            //var fwb = Bus.ReadRegisterShort((byte)Register.FW_BOOT_VERSION);
            var fwb = Peripheral.ReadRegister((byte)Register.FW_BOOT_VERSION);
            Console.WriteLine($"FWB version = 0x{fwb:x4}");

            //var fwa = Bus.ReadRegisterShort((byte)Register.FW_APP_VERSION);
            var fwa = Peripheral.ReadRegister((byte)Register.FW_APP_VERSION);
            Console.WriteLine($"FWA version = 0x{fwa:x4}");

            // read status
            //var status = Bus.ReadRegisterByte((byte)Register.STATUS);
            var status = Peripheral.ReadRegister((byte)Register.STATUS);
            Console.WriteLine($"status = 0x{status:x2}");
        }

        public ushort GetBaseline()
        {
            //return Bus.ReadRegisterShort((byte)Register.BASELINE);
            return Peripheral.ReadRegister((byte)Register.BASELINE);

        }

        public void SetBaseline(ushort value)
        {
            //Bus.WriteRegister((byte)Register.BASELINE, value);
            Peripheral.WriteRegister((byte)Register.BASELINE, (byte)value);
        }

        public MeasurementMode GetMeasurementMode()
        {
            //return (MeasurementMode)Bus.ReadRegisterByte((byte)Register.MEAS_MODE);
            return (MeasurementMode)Peripheral.ReadRegister((byte)Register.MEAS_MODE);
        }

        public void SetMeasurementMode(MeasurementMode mode)
        {
            // TODO: interrupts, etc would be here
            var m = (byte)mode;
            //Bus.WriteRegister((byte)Register.MEAS_MODE, m);
            Peripheral.WriteRegister((byte)Register.MEAS_MODE, m);
        }

        private void Reset()
        {
            //Bus.WriteBytes((byte)Register.SW_RESET, 0x11, 0xE5, 0x72, 0x8A);
            Peripheral.Write(new byte[] { (byte)Register.SW_RESET, 0x11, 0xE5, 0x72, 0x8A });
        }

        protected override async Task<(Concentration? Co2, Concentration? Voc)> ReadSensor()
        {
            return await Task.Run(() =>
            {
                // data is really in just the first 4, but this gets us status and raw data as well
                Peripheral.ReadRegister((byte)Register.ALG_RESULT_DATA, _readingBuffer);

                (Concentration co2, Concentration voc) state;
                state.co2 = new Concentration(_readingBuffer[0] << 8 | _readingBuffer[1], Concentration.UnitType.PartsPerMillion);
                state.voc = new Concentration(_readingBuffer[2] << 8 | _readingBuffer[3], Concentration.UnitType.PartsPerBillion);

                return state;
            });
        }

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