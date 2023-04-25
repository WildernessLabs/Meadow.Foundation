namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Ms5611
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x5C
            /// </summary>
            Address_0x5C = 0x5C,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x5C
        }

        /// <summary>
        /// MS5611 resolution
        /// </summary>
        public enum Resolution
        {
            /// <summary>
            /// OSR 256
            /// </summary>
            OSR_256 = 0,
            /// <summary>
            /// OSR 512
            /// </summary>
            OSR_512 = 1,
            /// <summary>
            /// OSR 1024
            /// </summary>
            OSR_1024 = 2,
            /// <summary>
            /// OSR 2048
            /// </summary>
            OSR_2048 = 3,
            /// <summary>
            /// OSR 4096
            /// </summary>
            OSR_4096 = 4
        }

        enum Commands : byte
        {
            Reset = 0x1e,
            ConvertD1 = 0x40,
            ConvertD2 = 0x50,
            ReadADC = 0x00,
        }
    }
}
