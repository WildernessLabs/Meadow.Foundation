namespace Meadow.Foundation.Graphics
{
    public class Font4x8 : FontBase
    {
        #region Constants

        /// <summary>
        ///     Width of the font in pixels.
        /// </summary>
        private const int WIDTH = 4;

        /// <summary>
        ///     Height of the font in pixels.
        /// </summary>
        private const int HEIGHT = 8;

        #endregion Constants

        #region Properties

        /// <summary>
        ///     Width of a character in the font.
        /// </summary>
        public override int Width
        {
            get { return WIDTH; }
        }

        /// <summary>
        ///     /   Height of a character in the font.
        /// </summary>
        public override int Height
        {
            get { return HEIGHT; }
        }

        #endregion Properties

        #region Member variables / fields

        /// <summary>
        ///     Font table containing the binary representation of ASCII characters.
        /// </summary>
        private static readonly byte[][] _fontTable =
        {
            #region Font codes

            new byte[] {0x00, 0x00, 0x00, 0x00, }, // U+0020 (space)
            new byte[] {0x22, 0x22, 0x20, 0x00, }, // U+0021 (!)
            new byte[] {0x55, 0x00, 0x00, 0x00, }, // U+0022 (")
            new byte[] {0x75, 0x75, 0x05, 0x00, }, // U+0023 (#)
            new byte[] {0x62, 0x61, 0x34, 0x02, }, // U+0024 ($)
            new byte[] {0x45, 0x22, 0x51, 0x00, }, // U+0025 (%)
            new byte[] {0x17, 0x13, 0x75, 0x00, }, // U+0026 (&)
            new byte[] {0x22, 0x00, 0x00, 0x00, }, // U+0027 (')
            new byte[] {0x24, 0x22, 0x42, 0x00, }, // U+0028 (()
            new byte[] {0x21, 0x22, 0x12, 0x00, }, // U+0029 ())
            new byte[] {0x25, 0x05, 0x00, 0x00, }, // U+002A (*)
            new byte[] {0x20, 0x27, 0x00, 0x00, }, // U+002B (+)
            new byte[] {0x00, 0x00, 0x10, 0x01, }, // U+002C (,)
            new byte[] {0x00, 0x07, 0x00, 0x00, }, // U+002D (-)
            new byte[] {0x00, 0x00, 0x20, 0x00, }, // U+002E (.)
            new byte[] {0x44, 0x22, 0x11, 0x00, }, // U+002F (/)
            new byte[] {0x52, 0x55, 0x25, 0x00, }, // U+0030 (0)
            new byte[] {0x22, 0x22, 0x22, 0x00, }, // U+0031 (1)
            new byte[] {0x43, 0x12, 0x71, 0x00, }, // U+0032 (2)
            new byte[] {0x43, 0x43, 0x34, 0x00, }, // U+0033 (3)
            new byte[] {0x55, 0x75, 0x44, 0x00, }, // U+0034 (4)
            new byte[] {0x17, 0x43, 0x34, 0x00, }, // U+0035 (5)
            new byte[] {0x16, 0x53, 0x25, 0x00, }, // U+0036 (6)
            new byte[] {0x47, 0x24, 0x22, 0x00, }, // U+0037 (7)
            new byte[] {0x57, 0x63, 0x75, 0x00, }, // U+0038 (8)
            new byte[] {0x52, 0x65, 0x34, 0x00, }, // U+0039 (9)
            new byte[] {0x00, 0x02, 0x20, 0x00, }, // U+003A (:)
            new byte[] {0x00, 0x02, 0x20, 0x02, }, // U+003B (//)
            new byte[] {0x40, 0x12, 0x42, 0x00, }, // U+003C (<)
            new byte[] {0x00, 0x07, 0x07, 0x00, }, // U+003D (=)
            new byte[] {0x10, 0x42, 0x12, 0x00, }, // U+003E (>)
            new byte[] {0x52, 0x24, 0x20, 0x00, }, // U+003F (?)
            new byte[] {0x52, 0x77, 0x61, 0x00, }, // U+0040 (@)
            new byte[] {0x52, 0x75, 0x55, 0x00, }, // U+0041 (A)
            new byte[] {0x57, 0x53, 0x75, 0x00, }, // U+0042 (B)
            new byte[] {0x16, 0x11, 0x61, 0x00, }, // U+0043 (C)
            new byte[] {0x53, 0x55, 0x35, 0x00, }, // U+0044 (D)
            new byte[] {0x17, 0x13, 0x71, 0x00, }, // U+0045 (E)
            new byte[] {0x17, 0x13, 0x11, 0x00, }, // U+0046 (F)
            new byte[] {0x16, 0x51, 0x35, 0x00, }, // U+0047 (G)
            new byte[] {0x55, 0x57, 0x55, 0x00, }, // U+0048 (H)
            new byte[] {0x27, 0x22, 0x72, 0x00, }, // U+0049 (I)
            new byte[] {0x44, 0x44, 0x25, 0x00, }, // U+004A (J)
            new byte[] {0x55, 0x35, 0x55, 0x00, }, // U+004B (K)
            new byte[] {0x11, 0x11, 0x71, 0x00, }, // U+004C (L)
            new byte[] {0x75, 0x57, 0x55, 0x00, }, // U+004D (M)
            new byte[] {0x54, 0x77, 0x15, 0x00, }, // U+004E (N)
            new byte[] {0x52, 0x55, 0x25, 0x00, }, // U+004F (O)
            new byte[] {0x53, 0x35, 0x11, 0x00, }, // U+0050 (P)
            new byte[] {0x52, 0x55, 0x25, 0x04, }, // U+0051 (Q)
            new byte[] {0x52, 0x35, 0x55, 0x00, }, // U+0052 (R)
            new byte[] {0x16, 0x43, 0x34, 0x00, }, // U+0053 (S)
            new byte[] {0x27, 0x22, 0x22, 0x00, }, // U+0054 (T)
            new byte[] {0x55, 0x55, 0x25, 0x00, }, // U+0055 (U)
            new byte[] {0x55, 0x55, 0x22, 0x00, }, // U+0056 (V)
            new byte[] {0x55, 0x75, 0x57, 0x00, }, // U+0057 (W)
            new byte[] {0x55, 0x52, 0x55, 0x00, }, // U+0058 (X)
            new byte[] {0x55, 0x27, 0x22, 0x00, }, // U+0059 (Y)
            new byte[] {0x47, 0x12, 0x71, 0x00, }, // U+005A (Z)
            new byte[] {0x26, 0x22, 0x62, 0x00, }, // U+005B ([)
            new byte[] {0x11, 0x22, 0x44, 0x00, }, // U+005C (\)
            new byte[] {0x23, 0x22, 0x32, 0x00, }, // U+005D (])
            new byte[] {0x52, 0x00, 0x00, 0x00, }, // U+005E (^)
            new byte[] {0x00, 0x00, 0x00, 0x07, }, // U+005F (_)
            new byte[] {0x21, 0x00, 0x00, 0x00, }, // U+0060 (`)
            new byte[] {0x00, 0x56, 0x65, 0x00, }, // U+0061 (a)
            new byte[] {0x11, 0x53, 0x35, 0x00, }, // U+0062 (b)
            new byte[] {0x00, 0x16, 0x61, 0x00, }, // U+0063 (c)
            new byte[] {0x44, 0x56, 0x65, 0x00, }, // U+0064 (d)
            new byte[] {0x00, 0x52, 0x63, 0x00, }, // U+0065 (e)
            new byte[] {0x24, 0x27, 0x22, 0x00, }, // U+0066 (f)
            new byte[] {0x00, 0x56, 0x65, 0x34, }, // U+0067 (g)
            new byte[] {0x11, 0x53, 0x55, 0x00, }, // U+0068 (h)
            new byte[] {0x02, 0x22, 0x22, 0x00, }, // U+0069 (i)
            new byte[] {0x04, 0x44, 0x54, 0x02, }, // U+006A (j)
            new byte[] {0x11, 0x35, 0x55, 0x00, }, // U+006B (k)
            new byte[] {0x22, 0x22, 0x22, 0x00, }, // U+006C (l)
            new byte[] {0x00, 0x77, 0x55, 0x00, }, // U+006D (m)
            new byte[] {0x00, 0x53, 0x55, 0x00, }, // U+006E (n)
            new byte[] {0x00, 0x52, 0x25, 0x00, }, // U+006F (o)
            new byte[] {0x00, 0x53, 0x35, 0x11, }, // U+0070 (p)
            new byte[] {0x00, 0x56, 0x65, 0x44, }, // U+0071 (q)
            new byte[] {0x00, 0x35, 0x11, 0x00, }, // U+0072 (r)
            new byte[] {0x00, 0x16, 0x36, 0x00, }, // U+0073 (s)
            new byte[] {0x22, 0x27, 0x22, 0x00, }, // U+0074 (t)
            new byte[] {0x00, 0x55, 0x65, 0x00, }, // U+0075 (u)
            new byte[] {0x00, 0x55, 0x22, 0x00, }, // U+0076 (v)
            new byte[] {0x00, 0x55, 0x77, 0x00, }, // U+0077 (w)
            new byte[] {0x00, 0x25, 0x55, 0x00, }, // U+0078 (x)
            new byte[] {0x00, 0x55, 0x22, 0x11, }, // U+0079 (y)
            new byte[] {0x00, 0x47, 0x73, 0x00, }, // U+007A (z)
            new byte[] {0x26, 0x32, 0x62, 0x00, }, // U+007B ({)
            new byte[] {0x22, 0x22, 0x22, 0x02, }, // U+007C (|)
            new byte[] {0x23, 0x62, 0x32, 0x00, }, // U+007D (})
            new byte[] {0x5a, 0x00, 0x00, 0x00, }, // U+007E (~)
            new byte[] {0x00, 0x00, 0x00, 0x00, }, // U+007F

            #endregion Font codes
        };

        #endregion Member variables / fields

        #region Methods

        /// <summary>
        ///     Get the binary representation of an ASCII character from the
        ///     font table.
        /// </summary>
        /// <param name="character">Character to look up.</param>
        /// <returns>
        ///     Byte array containing the rows of pixels in the character.  Unknown byte codes will result in a space being
        ///     returned.
        /// </returns>
        public override byte[] this[char character]
        {
            get
            {
                var index = (byte)character;
                if ((index < 32) || (index > 127))
                {
                    return _fontTable[0x20];
                }
                return _fontTable[(byte)character - 0x20];
            }
        }

        #endregion Methods
    }
}