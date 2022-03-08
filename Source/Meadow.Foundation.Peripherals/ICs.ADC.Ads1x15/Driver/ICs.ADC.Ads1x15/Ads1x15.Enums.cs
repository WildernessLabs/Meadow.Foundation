namespace Meadow.Foundation.ICs.ADC
{
    public partial class Ads1x15
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address48 = 0x48,
            Address49 = 0x49,
            Address4A = 0x4A,
            Address4B = 0x4C,
            Default = Address48
        }

        public enum FsrGain
        {
            TwoThirds = 0x00,
            One = 0x01,
            Two = 0x02,
            Four = 0x03,
            Eight = 0x04,
            Sixteen = 0x05
        }

        public enum MeasureMode
        {
            Continuous = 0x00,
            OneShot = 0x01
        }

        internal enum Register : byte
        {
            Conversion = 0x00,
            Config = 0x01,
            LowThresh = 0x02,
            HighThresh = 0x03,
        }

        public enum ChannelSetting
        {
            A0A1Differential,
            A0A3Differential,
            A1A3Differential,
            A2A3Differential,
            A0SingleEnded,
            A1SingleEnded,
            A2SingleEnded,
            A3SingleEnded,
        }
    }
}
