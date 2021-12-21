using Meadow.Hardware;

namespace Meadow.Foundation.ICs.ADC
{
    public class Ads1015 : Ads1x15
    {
        public enum DataRateSetting
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

        public DataRateSetting SampleRate
        {
            get => (DataRateSetting)InternalSampleRate;
            set => InternalSampleRate = (int)value;
        }

        protected Ads1015(II2cBus i2cBus,
            byte address = (byte)Address.Default,
            MeasureMode mode = MeasureMode.OneShot)
            : base(i2cBus, address, mode)
        {

        }

    }
}