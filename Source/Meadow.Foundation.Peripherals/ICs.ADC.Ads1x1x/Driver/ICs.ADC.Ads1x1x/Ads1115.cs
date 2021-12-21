using Meadow.Hardware;

namespace Meadow.Foundation.ICs.ADC
{
    public class Ads1115 : Ads1x15
    {
        public enum DataRateSetting
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

        public DataRateSetting SampleRate 
        {
            get => (DataRateSetting)InternalSampleRate;
            set => InternalSampleRate = (int)value;
        }

        protected Ads1115(II2cBus i2cBus,
            byte address = (byte)Address.Default,
            MeasureMode mode = MeasureMode.OneShot)
            : base(i2cBus, address, mode)
        {

        }
    }
}