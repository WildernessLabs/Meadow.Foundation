using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// A Yaff Font that has all characters the same width.
    /// </summary>
    public abstract class YaffBaseFont : IYaffFont
    {
        private static readonly string bearing = ".................";

        /// <summary>
        /// Width of a character in the font
        /// </summary>
        public int Width { get; protected set; }

        /// <summary>
        /// Height of a character in the font
        /// </summary>
        public int Height { get; protected set; }

        /// <summary>
        /// This array provides compatability for IFont 
        /// Get the binary representation of the ASCII character from the font table
        /// 
        /// For Each Character, we need the glyph as bytes
        /// But it is only 1 byte per row if the font is 8 
        /// other widths need to be packed - this is why only 4 6 8 12 are valid        ///
        /// </summary>
        /// <param name="character">Character to look up</param>
        /// <returns>Array of bytes representing the binary bit pattern of the character</returns>
        public byte[] this[char character]
        {
            get
            {
                // Console.WriteLine($"Looking for {character}");

                // We need a bitarray for the whole character, not line by line
                var b = new BitArray(Width * Height);

                var yg = GetGlyph(character);
                if (yg.glyphs[0] == "-")
                {
                    // empty
                    b.SetAll(false);
                }
                else
                {
                    int yy = 0;
                    foreach (var g in yg.glyphs)
                    {
                        string modg = g;
                        // right or left bearing ? 
                        if (yg.props.TryGetValue("left-bearing", out var lb))
                        {
                            if (int.TryParse(lb, out var ilb))
                                if (ilb > 0)
                                    modg = bearing[..ilb] + modg;
                        }

                        if (yg.props.TryGetValue("right-bearing", out var rb))
                        {
                            if (int.TryParse(rb, out var irb))
                                if (irb > 0)
                                    modg += bearing[..irb];
                        }

                        //Console.WriteLine(modg);
                        int xx = 0;
                        foreach (var c in modg)
                        {
                            b.Set(yy * Width + xx, c == YaffConst.ink);
                            xx++;
                        }
                        yy++;
                    }
                }

                return BitArrayToByteArray(b);
            }
        }

        private static byte[] BitArrayToByteArray(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        // Yaff Extentions

        /// <summary>
        /// Only certain sizes of fixed fonts are compatible with the binary representation of IFont
        /// if this is true DrawText can render... else DrawYaffText should be used
        /// </summary>
        public virtual bool IsIFontCompatible { get; }
        
        /// <summary>
        /// The Name of the Yaff Font
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Proprotional or fixed
        /// </summary>
        public YaffFontType Type { get; protected set; }

        /// <summary>
        /// All the characters representented by this font
        /// This makes it easier to tell which characters have Gyphs available
        /// </summary>
        public List<char> CharMap
        {
            get
            {
                var cm = new List<char>();
                foreach (var ch in glyphs.Keys)
                    cm.Add((char)ch);
                return cm;
            }
        }

        /// <summary>
        /// Glyphs by unicode character
        /// </summary>
        protected readonly SortedDictionary<uint, YaffGlyph> glyphs = new SortedDictionary<uint, YaffGlyph>();

        /// <summary>
        /// Glyphs by special names
        /// </summary>
        protected readonly SortedDictionary<string, YaffGlyph> namedglyphs = new SortedDictionary<string, YaffGlyph>();

        /// <summary>
        /// Create the Font from the pieces
        /// </summary>        /// 
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <param name="name">name of font</param>
        /// <param name="g">glyphs</param>
        public YaffBaseFont(int width, int height, string name, List<YaffGlyph> g)
        {
            Width = width;
            Height = height;

            Name = name;
            Type = YaffFontType.None;

            // Parse the passed in Glyph collection - we want access by ascii char number / unicoode
            foreach (var gg in g)
            {
                foreach (var t in gg.labels)
                {
                    string tt = t.Trim('"').ToLowerInvariant();
                    // hex or unicode
                    if (tt.StartsWith("0x") || tt.StartsWith("u+"))
                    {
                        var utt = tt[2..].ToUpperInvariant();
                        if (uint.TryParse(utt,
                                          NumberStyles.AllowHexSpecifier,
                                          CultureInfo.InvariantCulture,
                                          out uint index) && !glyphs.ContainsKey(index))
                            glyphs.Add(index, gg);
                        else if (!namedglyphs.ContainsKey(tt))
                            namedglyphs.Add(tt, gg);
                    }
                    // decimal numeric
                    else if (uint.TryParse(tt, out uint index) && !glyphs.ContainsKey(index))
                        glyphs.Add(index, gg);
                    // string character or name
                    else if (!namedglyphs.ContainsKey(tt))
                        namedglyphs.Add(tt, gg);
                }
            }
        }

        /// <summary>
        /// return the glyph as vertical lines instead of horizontal
        /// </summary>
        /// <param name="c">character to get lines for</param>
        /// <returns>List of ink/paper lines</returns>
        public List<string> GlyphLines(char c)
        {
            var yg = GetGlyph(c);

            var height = yg.glyphs.Count;
            var width = yg.glyphs[0].Length;

            var result = new List<string>();

            // return vertical strips of the ink and empty 
            for (var i = 0; i < width; i++)
            {
                var line = new string(' ', height).ToCharArray();
                for (var y = 0; y < height; y++)
                    line[y] = yg.glyphs[y][i];

                result.Add(new string(line));
            }

            return result;
        }

        /// <summary>
        /// return Gylphs for the requested character or defaults specified by the font
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        protected YaffGlyph GetGlyph(char character)
        {
            if (glyphs.TryGetValue(character, out YaffGlyph chval))
                return chval;
            else if (namedglyphs.TryGetValue("default", out YaffGlyph defval))
                return defval;
            else if (namedglyphs.TryGetValue("empty", out YaffGlyph emval))
                return emval;
            else if (glyphs.ContainsKey(' '))
                return glyphs[' '];
            else
                return glyphs[glyphs.Keys.Last()];
        }

        /// <summary>
        /// how much bearing to add before and after the character ( can be -ve !)
        /// </summary>
        /// <param name="c">character to get bearing for</param>
        /// <returns>tuple of bearings</returns>
        public (int lb, int rb) GetBearing(char c)
        {
            int lb = 0;
            int rb = 0;
            var yg = GetGlyph(c);

            if (yg.props.TryGetValue("left-bearing", out var lbearing))
                _ = int.TryParse(lbearing, out lb);

            if (yg.props.TryGetValue("right-bearing", out var rbearing))
                _ = int.TryParse(rbearing, out rb);

            return (lb, rb);
        }

        /// <summary>
        /// How wide is this character, without bearing - expect fixed fonts to have the same width for all characters
        /// </summary>
        /// <param name="c">character</param>
        /// <returns>int</returns>
        public int GetWidth(char c)
        {
            var yg = GetGlyph(c);
            return yg.glyphs[0].Length;
        }

        /// <summary>
        /// How tall is this character - expext all characters to be the same height
        /// </summary>
        /// <param name="c">character</param>
        /// <returns>int</returns>
        public int GetHeight(char c)
        {
            var yg = GetGlyph(c);
            return yg.glyphs.Count;
        }
    }

    /// <summary>
    /// Represents a Single Character in Yaff Format
    /// </summary>
    public class YaffGlyph
    {
        /// <summary>
        /// Comments on the character (if any)
        /// </summary>
        public string comment { get; set; }

        /// <summary>
        /// dictionary of properties
        /// </summary>
        public Dictionary<string, string> props { get; } = new Dictionary<string, string>();

        /// <summary>
        /// unicode codepage tag
        /// </summary>
        public List<string> labels { get; } = new List<string>();

        /// <summary>
        /// ink paper empty as string
        /// </summary>
        public List<string> glyphs { get; } = new List<string>();
    }
}
