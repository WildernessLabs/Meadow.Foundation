namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23x08
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x20
            /// </summary>
            Address_0x20 = 0x20,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x20
        }
    }
}