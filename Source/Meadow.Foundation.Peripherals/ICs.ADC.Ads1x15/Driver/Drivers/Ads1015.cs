using Meadow.Hardware;

namespace Meadow.Foundation.ICs.ADC
{
    /// <summary>
    /// Represents a ADS1015 12-bit, 3.3-kSPS, 4-channel, delta-sigma analog-to-digital converter with PGA, oscillator, VREF, comparator
    /// </summary>
    public class Ads1015 : Ads1x15
    {
        /// <summary>
        /// Sample rate setting
        /// </summary>
        public enum SampleRateSetting
        {
            /// <summary>
            /// 128
            /// </summary>
            Sps128,
            /// <summary>
            /// 250
            /// </summary>
            Sps250,
            /// <summary>
            /// 490
            /// </summary>
            Sps490,
            /// <summary>
            /// 920
            /// </summary>
            Sps920,
            /// <summary>
            /// 1600
            /// </summary>
            Sps1600,
            /// <summary>
            /// 2400
            /// </summary>
            Sps2400,
            /// <summary>
            /// 3300
            /// </summary>
            Sps3300
        }

        /// <summary>
        /// Sample rate resolution
        /// </summary>
        protected override int BitResolution => 12;

        /// <summary>
        /// Bits to shift for reads
        /// </summary>
        protected override int ReadShiftBits => 4;

        /// <summary>
        /// Sample rate setting
        /// </summary>
        public SampleRateSetting SampleRate
        {
            get => (SampleRateSetting)InternalSampleRate;
            set => InternalSampleRate = (int)value;
        }

        /// <summary>
        /// Create a new ADS1015 object
        /// </summary>
        public Ads1015(II2cBus i2cBus,
            Addresses address = Addresses.Default,
            MeasureMode mode = MeasureMode.OneShot,
            ChannelSetting channel = ChannelSetting.A0A1Differential,
            SampleRateSetting sampleRate = SampleRateSetting.Sps1600)
            : base(i2cBus, address, mode, channel)
        {
            SampleRate = sampleRate;
        }
    }
}