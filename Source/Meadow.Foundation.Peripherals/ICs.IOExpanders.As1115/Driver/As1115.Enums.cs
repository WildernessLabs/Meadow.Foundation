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

        /// <summary>
        /// The decode mode used for displaying pixels or characters
        /// </summary>
        public enum DecodeMode : byte
        {
            /// <summary>
            /// Hexicemial charcter encoding  for 7-segment displays
            /// characters 0 to 9, E, H, L, P, and -
            /// </summary>
            Hexidecimal,
            /// <summary>
            /// BCD character encoding for 7-segment displays
            ///  characters 0 to 9 and A to F
            /// </summary>
            BCD,
            /// <summary>
            /// Direct pixel mapping for 8x8 matrix displays (default)
            /// </summary>
            Pixel,
        }

        /// <summary>
        /// Key scan buttons
        /// </summary>
        public enum KeyScanButtonType : byte
        {
            /// <summary>
            /// Button 1 
            /// Key A: 127, Key B: 255
            /// </summary>
            Button1,
            /// <summary>
            /// Button 2
            /// Key A: 191, Key B: 255
            /// </summary>
            Button2,
            /// <summary>
            /// Button 3
            /// Key A: 223, Key B: 255
            /// </summary>
            Button3,
            /// <summary>
            /// Button 4
            /// Key A: 239, Key B: 255
            /// </summary>
            Button4,
            /// <summary>
            /// Button 5
            /// Key A: 247, Key B: 255
            /// </summary>
            Button5,
            /// <summary>
            /// Button 6
            /// Key A: 251, Key B: 255
            /// </summary>
            Button6,
            /// <summary>
            /// Button 7
            /// Key A: 253, Key B: 255
            /// </summary>
            Button7,
            /// <summary>
            /// Button 8
            /// Key A: 254, Key B: 255
            /// </summary>
            Button8,
            /// <summary>
            /// Button 9
            /// Key A: 255, Key B: 127
            /// </summary>
            Button9,
            /// <summary>
            /// Button 10
            /// Key A: 255, Key B: 191
            /// </summary>
            Button10,
            /// <summary>
            /// Button 11
            /// Key A: 255, Key B: 223
            /// </summary>
            Button11,
            /// <summary>
            /// Button 11
            /// Key A: 255, Key B: 239
            /// </summary>
            Button12,
            /// <summary>
            /// Button 12
            /// Key A: 255, Key B: 247
            /// </summary>
            Button13,
            /// <summary>
            /// Button 13
            /// Key A: 255, Key B: 251
            /// </summary>
            Button14,
            /// <summary>
            /// Button 15
            /// Key A: 255, Key B: 253
            /// </summary>
            Button15,
            /// <summary>
            /// Button 16
            /// Key A: 255, Key B: 254
            /// </summary>
            Button16,
            /// <summary>
            /// No button pressed or selected
            /// </summary>
            None,
        }
    }
}