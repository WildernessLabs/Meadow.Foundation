using Meadow.Hardware;

namespace Meadow.Foundation.ICs.ADC
{
    public class Ads1115 : Ads1x15
    {
        public enum SampleRateSetting
        {
            Sps8,
            Sps16,
            Sps32,
            Sps64,
            Sps128,
            Sps250,
            Sps475,
            Sps860
        }

        protected override int BitResolution => 16;

        public SampleRateSetting SampleRate
        {
            get => (SampleRateSetting)InternalSampleRate;
            set => InternalSampleRate = (int)value;
        }

        public Ads1115(II2cBus i2cBus,
            Addresses address = Addresses.Default,
            MeasureMode mode = MeasureMode.OneShot,
            ChannelSetting channel = ChannelSetting.A0A1Differential,
            SampleRateSetting sampleRate = SampleRateSetting.Sps128)

            : base(i2cBus, address, mode, channel)
        {
            SampleRate = sampleRate;
        }
    }
}