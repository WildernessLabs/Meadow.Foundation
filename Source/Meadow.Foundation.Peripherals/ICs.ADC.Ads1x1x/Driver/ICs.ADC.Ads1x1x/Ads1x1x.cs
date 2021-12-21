using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.ADC
{
    /// <summary>
    /// Encapsulation for ADCs based upon the Ads1x1x family of chips.
    /// </summary>
    public abstract partial class Ads1x15
    {
        private readonly II2cPeripheral _i2c;
        private ushort _config;

        private const int RateShift = 5;
        private const int RateMask = 0b111 << RateShift;
        private const int ModeShift = 8;
        private const int MuxShift = 12;

        protected abstract int BitResolution { get; }

        /// <summary>
        ///     Create a new Ads1x15 object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the At24Cxx (default = 0x50).</param>
        /// <param name="i2cBus"></param>
        /// <param name="mode"></param>
        protected Ads1x15(II2cBus i2cBus,
            byte address,
            MeasureMode mode)
        {
            _i2c = new I2cPeripheral(i2cBus, address);
            _config = _i2c.ReadRegisterAsUShort((byte)Register.Config);

            Mode = mode;
        }

        internal protected int InternalSampleRate
        {
            get => (_config >> RateShift) & 0b111;
            set
            {
                if (value == InternalSampleRate) return;

                ushort newConfig = (ushort)(_config & ~(0b111 << RateShift));
                newConfig = (ushort)(newConfig | (value << RateShift));

                _i2c.WriteRegister((byte)Register.Config, newConfig);
            }

        }

        public ChannelSetting Channel
        {
            get => (ChannelSetting)((_config >> MuxShift) & 0b111);
            set
            {
                if (value == Channel) return;

                ushort newConfig = (ushort)(_config & ~(0b111 << MuxShift));
                newConfig = (ushort)(newConfig | ((int)value << MuxShift));

                _i2c.WriteRegister((byte)Register.Config, newConfig);
            }

        }

        public MeasureMode Mode
        {
            get => (MeasureMode)(_config >> ModeShift & 1);
            set
            {
                if (value == Mode) return;

                ushort newConfig;
                
                if(value == MeasureMode.OneShot)
                {
                    newConfig = (ushort)(_config | (1 << ModeShift));
                }
                else
                {
                    newConfig = (ushort)(_config & ~(1 << ModeShift));
                }
                _i2c.WriteRegister((byte)Register.Config, newConfig);
            }
        }
        
        public async Task<int> Read()
        {
            if(Mode == MeasureMode.OneShot)
            {
                // trigger a conversion
                var cfg = (ushort)(_config | 1 << 15);
                _i2c.WriteRegister((byte)Register.Config, cfg);
                // wait for conversion complete (TODO: we could read for completion - it's always < 1ms)
                await Task.Delay(1);

            }
            return _i2c.ReadRegisterAsUShort((byte)Register.Conversion);
        }
    }
}