using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// A Yaff Font that has all characters the same width.
    /// </summary>
    public class YaffFixedFont : YaffBaseFont
    {

        /// <summary>
        /// Create the Font from the pieces
        /// </summary>        /// 
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <param name="name">name of font</param>
        /// <param name="g">glyphs</param>
        public YaffFixedFont(int width, int height, string name, List<YaffGlyph> g) : base(0, 0, name, g)
        {
            Type = YaffFontType.Fixed;

            // For compatability with IFont a Fixed font needs to align on these boundaries
            // This will cause 1 pixel of extra space after the the character for non aigned fonts 
            Width = width;  // 4 6 8 12 for IFont
            Height = height; // if 6/12 only certain heights will work in ifont
        }

        /// <summary>
        /// Only certain sizes of fixed fonts are compatible with the binary representation of IFont
        /// if this is true DrawText can render... else DrawYaffText should be used
        /// </summary>
        public override bool IsIFontCompatible
        {
            get
            {
                if (Width != 4 && Width != 6 && Width != 8 && Width != 12)
                    return false;
                if ((Width == 6 || Width == 12) &&
                    (Height != 8 || Height != 12 || Height != 16))
                    return false;

                return true;
            }
        }
    }
} 
