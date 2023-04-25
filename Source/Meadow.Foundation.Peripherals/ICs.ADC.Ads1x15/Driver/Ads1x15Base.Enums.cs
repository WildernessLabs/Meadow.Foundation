namespace Meadow.Foundation.ICs.ADC
{
    public abstract partial class Ads1x15Base
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x48
            /// </summary>
            Address_0x48 = 0x48,
            /// <summary>
            /// Bus address 0x49
            /// </summary>
            Address_0x49 = 0x49,
            /// <summary>
            /// Bus address 0x4A
            /// </summary>
            Address_0x4A = 0x4A,
            /// <summary>
            /// Bus address 0x4C
            /// </summary>
            Address_0x4B = 0x4C,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x48
        }

        /// <summary>
        /// FSR Grain
        /// </summary>
        public enum FsrGain
        {
            /// <summary>
            /// 2/3rds
            /// </summary>
            TwoThirds = 0x00,
            /// <summary>
            /// 1
            /// </summary>
            One = 0x01,
            /// <summary>
            /// 2
            /// </summary>
            Two = 0x02,
            /// <summary>
            /// 4
            /// </summary>
            Four = 0x03,
            /// <summary>
            /// 8
            /// </summary>
            Eight = 0x04,
            /// <summary>
            /// 16
            /// </summary>
            Sixteen = 0x05
        }

        /// <summary>
        /// Measuring mode
        /// </summary>
        public enum MeasureMode
        {
            /// <summary>
            /// Continuous measurement
            /// </summary>
            Continuous = 0x00,
            /// <summary>
            /// Single measurement
            /// </summary>
            OneShot = 0x01
        }

        internal enum Register : byte
        {
            Conversion = 0x00,
            Config = 0x01,
            LowThresh = 0x02,
            HighThresh = 0x03,
        }

        /// <summary>
        /// Channel setting
        /// </summary>
        public enum ChannelSetting
        {
            /// <summary>
            /// A0A1 Differential
            /// </summary>
            A0A1Differential,
            /// <summary>
            /// A0A3 Differential
            /// </summary>
            A0A3Differential,
            /// <summary>
            /// A1A3 Differential
            /// </summary>
            A1A3Differential,
            /// <summary>
            /// A2A3 Differential
            /// </summary>
            A2A3Differential,
            /// <summary>
            /// A0 Single Ended
            /// </summary>
            A0SingleEnded,
            /// <summary>
            /// A1 Single Ended
            /// </summary>
            A1SingleEnded,
            /// <summary>
            /// A2 Single Ended
            /// </summary>
            A2SingleEnded,
            /// <summary>
            /// A3 Single Ended
            /// </summary>
            A3SingleEnded,
        }
    }
}