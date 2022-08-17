namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23x08
    {
        /// <summary>
        /// Valid addresses for the sensor
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x20
            /// </summary>
            Address_0x20 = 0x20,
            /// <summary>
            /// Bus address 0x21
            /// </summary>
            Address_0x21 = 0x21,
            /// <summary>
            /// Bus address 0x22
            /// </summary>
            Address_0x22 = 0x22,
            /// <summary>
            /// Bus address 0x23
            /// </summary>
            Address_0x23 = 0x23,
            /// <summary>
            /// Bus address 0x24
            /// </summary>
            Address_0x24 = 0x24,
            /// <summary>
            /// Bus address 0x25
            /// </summary>
            Address_0x25 = 0x25,
            /// <summary>
            /// Bus address 0x26
            /// </summary>
            Address_0x26 = 0x26,
            /// <summary>
            /// Bus address 0x27
            /// </summary>
            Address_0x27 = 0x27,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x20
        }
    }
}