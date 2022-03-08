using Meadow.Hardware;

namespace Meadow.Foundation.ICs.ADC
{
    public class Ads1015 : Ads1x15
    {
        public enum SampleRateSetting
        {
            Sps128,
            Sps250,
            Sps490,
            Sps920,
            Sps1600,
            Sps2400,
            Sps3300
        }

        protected override int BitResolution => 12;
        protected override int ReadShiftBits => 4;

        public SampleRateSetting SampleRate
        {
            get => (SampleRateSetting)InternalSampleRate;
            set => InternalSampleRate = (int)value;
        }

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