namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class As1115
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
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
        public enum DecodeType : byte
        {
            /// <summary>
            /// Hexadecimal character encoding  for 7-segment displays
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

        /// <summary>
        /// BCD Character type
        /// </summary>
        public enum BcdCharacterType : byte
        {
            /// <summary>
            /// Zero (0)
            /// </summary>
            _0 = 0x00,
            /// <summary>
            /// One (1) 
            /// </summary>
            _1 = 0x01,
            /// <summary>
            /// Two (2)
            /// </summary>
            _2 = 0x02,
            /// <summary>
            /// Three (3)
            /// </summary>
            _3 = 0x03,
            /// <summary>
            /// Four (4)
            /// </summary>
            _4 = 0x04,
            /// <summary>
            /// Five (5)
            /// </summary>
            _5 = 0x05,
            /// <summary>
            /// Six (6)
            /// </summary>
            _6 = 0x06,
            /// <summary>
            /// Seven (7)
            /// </summary>
            _7 = 0x07,
            /// <summary>
            /// Eight (8)
            /// </summary>
            _8 = 0x08,
            /// <summary>
            /// Nine (9)
            /// </summary>
            _9 = 0x09,
            /// <summary>
            /// Hyphen (-)
            /// </summary>
            Hyphen = 0x0A,
            /// <summary>
            /// E
            /// </summary>
            E = 0x0B,
            /// <summary>
            /// H
            /// </summary>
            H = 0x0C,
            /// <summary>
            /// L
            /// </summary>
            L = 0x0D,
            /// <summary>
            /// P
            /// </summary>
            P = 0x0E,
            /// <summary>
            /// Space ( )
            /// </summary>
            Blank = 0x0F,
            /// <summary>
            /// Count of characters
            /// </summary>
            Count = 16
        }

        /// <summary>
        /// Hex Character type
        /// </summary>
        public enum HexCharacterType : byte
        {
            /// <summary>
            /// Zero (0)
            /// </summary>
            _0 = 0x00,
            /// <summary>
            /// One (1) 
            /// </summary>
            _1 = 0x01,
            /// <summary>
            /// Two (2)
            /// </summary>
            _2 = 0x02,
            /// <summary>
            /// Three (3)
            /// </summary>
            _3 = 0x03,
            /// <summary>
            /// Four (4)
            /// </summary>
            _4 = 0x04,
            /// <summary>
            /// Five (5)
            /// </summary>
            _5 = 0x05,
            /// <summary>
            /// Six (6)
            /// </summary>
            _6 = 0x06,
            /// <summary>
            /// Seven (7)
            /// </summary>
            _7 = 0x07,
            /// <summary>
            /// Eight (8)
            /// </summary>
            _8 = 0x08,
            /// <summary>
            /// Nine (9)
            /// </summary>
            _9 = 0x09,
            /// <summary>
            /// A
            /// </summary>
            A = 0x0A,
            /// <summary>
            /// B
            /// </summary>
            B = 0x0B,
            /// <summary>
            /// C
            /// </summary>
            C = 0x0C,
            /// <summary>
            /// D
            /// </summary>
            D = 0x0D,
            /// <summary>
            /// E
            /// </summary>
            E = 0x0E,
            /// <summary>
            /// F
            /// </summary>
            F = 0x0F,
            /// <summary>
            /// Count of characters
            /// </summary>
            Count = 16
        }
    }
}