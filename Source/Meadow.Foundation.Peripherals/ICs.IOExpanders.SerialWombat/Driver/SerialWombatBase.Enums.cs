namespace Meadow.Foundation.ICs.IOExpanders
{

    public partial class SerialWombatBase
    {
        /// <summary>
        /// Valid addresses for the Chip.
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x6a
            /// </summary>
            Address_0x6a = 0x6a,
            /// <summary>
            /// Bus address 0x6c
            /// </summary>
            Address_0x6b = 0x6b,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x6b
        }
    }
}