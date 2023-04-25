using Meadow.Hardware;

namespace Meadow.Foundation.ICs.ADC
{
    /// <summary>
    /// Represents an ADS1115 16-bit, 860-SPS, 4-channel, delta-sigma analog-to-digital converter with PGA, oscillator, VREF, comparato
    /// </summary>
    public class Ads1115 : Ads1x15Base
    {
        /// <summary>
        /// Sample rate setting
        /// </summary>
        public enum SampleRateSetting
        {
            /// <summary>
            /// 8
            /// </summary>
            Sps8,
            /// <summary>
            /// 16
            /// </summary>
            Sps16,
            /// <summary>
            /// 32
            /// </summary>
            Sps32,
            /// <summary>
            /// 64
            /// </summary>
            Sps64,
            /// <summary>
            /// 128
            /// </summary>
            Sps128,
            /// <summary>
            /// 250
            /// </summary>
            Sps250,
            /// <summary>
            /// 475
            /// </summary>
            Sps475,
            /// <summary>
            /// 860
            /// </summary>
            Sps860
        }

        /// <summary>
        /// Sample resolution
        /// </summary>
        protected override int BitResolution => 16;

        /// <summary>
        /// Sample rate setting
        /// </summary>
        public SampleRateSetting SampleRate
        {
            get => (SampleRateSetting)InternalSampleRate;
            set => InternalSampleRate = (int)value;
        }

        /// <summary>
        /// Create a new ADS1115 object
        /// </summary>
        public Ads1115(II2cBus i2cBus,
            Address address = Address.Default,
            MeasureMode mode = MeasureMode.OneShot,
            ChannelSetting channel = ChannelSetting.A0A1Differential,
            SampleRateSetting sampleRate = SampleRateSetting.Sps128)

            : base(i2cBus, address, mode, channel)
        {
            SampleRate = sampleRate;
        }
    }
}