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

        public enum KeyScanButtonType : byte
        {
            Button1,
            Button2,
            Button3,
            Button4,
            Button5,
            Button6,
            Button7,
            Button8,
            Button9,
            Button10,
            Button11,
            Button12,
            Button13,
            Button14,
            Button15,
            Button16,
            None,
        }
    }
}