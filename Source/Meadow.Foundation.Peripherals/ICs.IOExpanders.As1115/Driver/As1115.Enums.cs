namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class As1115
    {
        /// <summary>
        /// Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x00
            /// </summary>
            Address_0x00 = 0x00,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x00
        }
    }
}