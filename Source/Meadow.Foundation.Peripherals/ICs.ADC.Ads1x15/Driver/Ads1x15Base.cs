using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.ICs.ADC
{
    /// <summary>
    /// Base class for the Ads1x15 family of analog-to-digital (ADC) converters
    /// </summary>
    public abstract partial class Ads1x15Base : PollingSensorBase<Voltage>, II2cPeripheral
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;

        private ushort config;

        // These are config register bit offsets
        private const int RateShift = 5;
        private const int ModeShift = 8;
        private const int GainShift = 9;
        private const int MuxShift = 12;

        /// <summary>
        /// Resolution of the peripheral
        /// </summary>
        protected abstract int BitResolution { get; }
        /// <summary>
        /// Shift required for the conversion register (see Data Sheet for details)
        /// </summary>
        protected virtual int ReadShiftBits { get; } = 0;

        /// <summary>
        /// Create a new Ads1x15Base object using the default parameters for the component
        /// </summary>
        /// <param name="address">Address of the At24Cxx (default = 0x50)</param>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="measureMode">The measurement measureMode</param>
        /// <param name="channelSetting">The channel setting</param>
        protected Ads1x15Base(II2cBus i2cBus,
            Addresses address,
            MeasureMode measureMode,
            ChannelSetting channelSetting)
        {
            i2cComms = new I2cCommunications(i2cBus, (byte)address, 3);

            SetConfigRegister(0x8583); // this is the default reset - force it in case it's not been reset
            config = GetConfigRegister();
            Mode = measureMode;
            Channel = channelSetting;
        }

        private ushort GetRegister(Register register)
        {
            var read = i2cComms.ReadRegisterAsUShort((byte)register, ByteOrder.BigEndian);
            return read;
        }

        private void SetRegister(Register register, ushort value)
        {
            var w = new byte[3];
            w[0] = (byte)register;
            w[1] = (byte)(value >> 8);
            w[2] = (byte)(value & 0xff);
            i2cComms.Write(w);
            config = value;
        }

        private ushort GetConfigRegister()
        {
            return GetRegister(Register.Config);
        }

        private void SetConfigRegister(ushort value)
        {
            SetRegister(Register.Config, value);
            config = value;
        }

        /// <summary>
        /// Get or set the internal sample rate
        /// </summary>
        internal protected int InternalSampleRate
        {
            get => (config >> RateShift) & 0b111;
            set
            {
                if (value == InternalSampleRate) return;

                ushort newConfig = (ushort)(config & ~(0b111 << RateShift));
                newConfig = (ushort)(newConfig | (value << RateShift));

                SetConfigRegister(newConfig);
            }

        }

        /// <summary>
        /// Gets or sets the ADC Channel settings (e.g. Single-Ended or Differential)
        /// </summary>
        public ChannelSetting Channel
        {
            get => (ChannelSetting)((config >> MuxShift) & 0b111);
            set
            {
                if (value == Channel) return;

                ushort newConfig = (ushort)(config & ~(0b111 << MuxShift));
                newConfig = (ushort)(newConfig | ((int)value << MuxShift));

                SetConfigRegister(newConfig);
            }
        }

        /// <summary>
        /// Gets or sets the ADC Amplifier Gain
        /// </summary>
        public FsrGain Gain
        {
            get => (FsrGain)((config >> GainShift) & 0b111);
            set
            {
                if (value == Gain) return;

                ushort newConfig = (ushort)(config & ~(0b111 << GainShift));
                newConfig = (ushort)(newConfig | ((int)value << GainShift));

                SetConfigRegister(newConfig);
            }
        }

        /// <summary>
        /// Sets or gets the Measurement Mode
        /// One-shot uses less power but is slower
        /// </summary>
        public MeasureMode Mode
        {
            get => (MeasureMode)(config >> ModeShift & 1);
            set
            {
                if (value == Mode) return;

                ushort newConfig;

                if (value == MeasureMode.OneShot)
                {
                    newConfig = (ushort)(config | (1 << ModeShift));
                }
                else
                {
                    newConfig = (ushort)(config & ~(1 << ModeShift));
                }
                SetConfigRegister(newConfig);
            }
        }

        /// <summary>
        /// Reads the last ADC Conversion as a Voltage based on current Gain settings
        /// </summary>
        /// <returns>The voltage</returns>
        protected override async Task<Voltage> ReadSensor()
        {
            var raw = await ReadRaw();
            var scale = 0d;

            switch (Gain)
            {
                case FsrGain.TwoThirds:
                    scale = 6.144d;
                    break;
                case FsrGain.One:
                    scale = 4.096d;
                    break;
                case FsrGain.Two:
                    scale = 2.048d;
                    break;
                case FsrGain.Four:
                    scale = 1.024d;
                    break;
                case FsrGain.Eight:
                    scale = 0.512d;
                    break;
                case FsrGain.Sixteen:
                    scale = 0.256d;
                    break;
            }
            return new Voltage(raw * (scale / (0x8000 >> ReadShiftBits)), Units.Voltage.UnitType.Volts);
        }

        /// <summary>
        /// Returns the last raw ADC conversion value
        /// </summary>
        /// <returns></returns>
        public async Task<int> ReadRaw()
        {
            if (Mode == MeasureMode.OneShot)
            {
                // trigger a conversion
                var cfg = (ushort)(config | 1 << 15);
                SetConfigRegister(cfg);

                // wait for conversion complete (MSB == 1)
                while ((GetConfigRegister() & 0x8000) == 0)
                {
                    await Task.Delay(1);
                }
            }

            var reg = GetRegister(Register.Conversion);
            return reg >> ReadShiftBits;
        }
    }
}