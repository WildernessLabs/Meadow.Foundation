namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Is31fl3731
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x74
            /// </summary>
            Address_0x74 = 0x74,
            /// <summary>
            /// Bus address 0x77
            /// </summary>
            Address_0x77 = 0x77,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x74
        }
    }
}
