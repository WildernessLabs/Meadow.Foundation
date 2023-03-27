using System.Collections.Generic;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// Proportial fonts have various widths, and some use bearing
    /// </summary>
    public class YaffPropFont : YaffBaseFont
    {
        /// <summary>
        /// Create the Font from the pieces
        /// </summary>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <param name="name">name of font</param>
        /// <param name="g">glyphs</param>
        public YaffPropFont(int width, int height, string name, List<YaffGlyph> g) : base(0, 0, name, g)
        {
            Type = YaffFontType.Proportional;

            if (width == 0 || height == 0)
            {
                // compute the widths from rhe first character if missing
                Width = g[0].glyphs[0].Length;
                Height = g[0].glyphs.Count;
            }
            else
            {
                Width = width;
                Height = height;
            }
        }

        /// <summary>
        /// Only certain sizes of fixed fonts are compatible
        /// </summary>
        public override bool IsIFontCompatible
        {
            get { return false; }
        }
    }
}
