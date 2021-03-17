namespace Meadow.Foundation.Graphics
{
    public class Font4x6 : FontBase
    {
        #region Constants

        /// <summary>
        ///     Width of the font in pixels.
        /// </summary>
        private const int WIDTH = 4;

        /// <summary>
        ///     Height of the font in pixels.
        /// </summary>
        private const int HEIGHT = 6;

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

            new byte[]{0x00, 0x00, 0x00}, //0020( )
            new byte[]{0x22, 0x02, 0x02}, //0021(!)
            new byte[]{0x55, 0x00, 0x00}, //0022(")
            new byte[]{0x75, 0x75, 0x05}, //0023(#)
            new byte[]{0x62, 0x61, 0x24}, //0024($)
            new byte[]{0x45, 0x12, 0x05}, //0025(%)
            new byte[]{0x17, 0x13, 0x07}, //0026(&)
            new byte[]{0x22, 0x00, 0x00}, //0027(')
            new byte[]{0x24, 0x22, 0x04}, //0028(()
            new byte[]{0x21, 0x22, 0x01}, //0029())
            new byte[]{0x25, 0x05, 0x00}, //002A(*)
            new byte[]{0x20, 0x27, 0x00}, //002B(+)
            new byte[]{0x00, 0x00, 0x11}, //002C(,)
            new byte[]{0x00, 0x07, 0x00}, //002D(-)
            new byte[]{0x00, 0x00, 0x01}, //002E(.)
            new byte[]{0x44, 0x12, 0x01}, //002F(/)
            new byte[]{0x57, 0x55, 0x07}, //0030(0)
            new byte[]{0x22, 0x22, 0x02}, //0031(1)
            new byte[]{0x47, 0x17, 0x07}, //0032(2)
            new byte[]{0x47, 0x47, 0x07}, //0033(3)
            new byte[]{0x55, 0x47, 0x04}, //0034(4)
            new byte[]{0x17, 0x47, 0x07}, //0035(5)
            new byte[]{0x17, 0x57, 0x07}, //0036(6)
            new byte[]{0x47, 0x44, 0x04}, //0037(7)
            new byte[]{0x57, 0x57, 0x07}, //0038(8)
            new byte[]{0x57, 0x47, 0x07}, //0039(9)
            new byte[]{0x00, 0x02, 0x02}, //003A(:)
            new byte[]{0x00, 0x02, 0x22}, //003B(;)
            new byte[]{0x24, 0x21, 0x04}, //003C(<)
            new byte[]{0x70, 0x70, 0x00}, //003D(=)
            new byte[]{0x21, 0x24, 0x01}, //003E(>)
            new byte[]{0x47, 0x06, 0x02}, //003F(?)
            new byte[]{0x72, 0x35, 0x07}, //0040(@)
            new byte[]{0x57, 0x57, 0x05}, //0041(A)
            new byte[]{0x53, 0x57, 0x07}, //0042(B)
            new byte[]{0x17, 0x11, 0x07}, //0043(C)
            new byte[]{0x53, 0x55, 0x07}, //0044(D)
            new byte[]{0x17, 0x13, 0x07}, //0045(E)
            new byte[]{0x17, 0x13, 0x01}, //0046(F)
            new byte[]{0x17, 0x55, 0x07}, //0047(G)
            new byte[]{0x55, 0x57, 0x05}, //0048(H)
            new byte[]{0x27, 0x22, 0x07}, //0049(I)
            new byte[]{0x44, 0x54, 0x07}, //004A(J)
            new byte[]{0x55, 0x53, 0x05}, //004B(K)
            new byte[]{0x11, 0x11, 0x07}, //004C(L)
            new byte[]{0x75, 0x55, 0x05}, //004D(M)
            new byte[]{0x54, 0x57, 0x01}, //004E(N)
            new byte[]{0x57, 0x55, 0x07}, //004F(O)
            new byte[]{0x57, 0x17, 0x01}, //0050(P)
            new byte[]{0x57, 0x55, 0x47}, //0051(Q)
            new byte[]{0x53, 0x53, 0x05}, //0052(R)
            new byte[]{0x16, 0x47, 0x07}, //0053(S)
            new byte[]{0x27, 0x22, 0x02}, //0054(T)
            new byte[]{0x55, 0x55, 0x07}, //0055(U)
            new byte[]{0x55, 0x55, 0x02}, //0056(V)
            new byte[]{0x55, 0x75, 0x05}, //0057(W)
            new byte[]{0x55, 0x52, 0x05}, //0058(X)
            new byte[]{0x55, 0x22, 0x02}, //0059(Y)
            new byte[]{0x47, 0x12, 0x07}, //005A(Z)
            new byte[]{0x26, 0x22, 0x06}, //005B([)
            new byte[]{0x11, 0x42, 0x04}, //005C(\)
            new byte[]{0x23, 0x22, 0x03}, //005D(])
            new byte[]{0x52, 0x00, 0x00}, //005E(^)
            new byte[]{0x00, 0x00, 0x70}, //005F(_)
            new byte[]{0x21, 0x00, 0x00}, //0060(`)
            new byte[]{0x60, 0x55, 0x06}, //0061(a)
            new byte[]{0x31, 0x55, 0x03}, //0062(b)
            new byte[]{0x60, 0x11, 0x06}, //0063(c)
            new byte[]{0x64, 0x55, 0x06}, //0064(d)
            new byte[]{0x20, 0x35, 0x06}, //0065(e)
            new byte[]{0x16, 0x13, 0x01}, //0066(f)
            new byte[]{0x60, 0x65, 0x24}, //0067(g)
            new byte[]{0x11, 0x53, 0x05}, //0068(h)
            new byte[]{0x02, 0x22, 0x02}, //0069(i)
            new byte[]{0x02, 0x22, 0x12}, //006A(j)
            new byte[]{0x51, 0x53, 0x05}, //006B(k)
            new byte[]{0x22, 0x22, 0x02}, //006C(l)
            new byte[]{0x50, 0x57, 0x05}, //006D(m)
            new byte[]{0x30, 0x55, 0x05}, //006E(n)
            new byte[]{0x20, 0x55, 0x02}, //006F(o)
            new byte[]{0x30, 0x55, 0x13}, //0070(p)
            new byte[]{0x60, 0x55, 0x46}, //0071(q)
            new byte[]{0x50, 0x13, 0x01}, //0072(r)
            new byte[]{0x60, 0x43, 0x03}, //0073(s)
            new byte[]{0x72, 0x22, 0x02}, //0074(t)
            new byte[]{0x50, 0x55, 0x06}, //0075(u)
            new byte[]{0x50, 0x55, 0x02}, //0076(v)
            new byte[]{0x50, 0x75, 0x05}, //0077(w)
            new byte[]{0x50, 0x52, 0x05}, //0078(x)
            new byte[]{0x50, 0x25, 0x02}, //0079(y)
            new byte[]{0x70, 0x24, 0x07}, //007A(z)
            new byte[]{0x26, 0x23, 0x06}, //007B({)
            new byte[]{0x22, 0x22, 0x22}, //007C(|)
            new byte[]{0x23, 0x26, 0x03}, //007D(})
            new byte[]{0x5A, 0x00, 0x00}, //007E(~)
            new byte[]{0x00, 0x00, 0x00}, //00A0( )

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