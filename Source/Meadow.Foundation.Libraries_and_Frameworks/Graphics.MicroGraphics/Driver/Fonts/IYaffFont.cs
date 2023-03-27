using Meadow.Foundation.Graphics;
using System.Collections.Generic;

 namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// IYaffFont is an Extension to IFont that includes the Font Gylphs and bearing used in proprotional fonts
    /// </summary>
    public interface IYaffFont : IFont
    {
        /// <summary>
        /// The Name of the Font from the Yaff Contents
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The type is propoptional or fixed
        /// </summary>
        YaffFontType Type { get; }

        /// <summary>
        /// All the characters representented by this font
        /// This makes it easier to tell which characters have Gyphs available
        /// </summary>
        List<char> CharMap { get; }

        /// <summary>
        /// Only certain sizes of fixed fonts are compatible with the binary representation of IFont
        /// if this is true DrawText can render... else DrawYaffText should be used
        /// </summary>
        bool IsIFontCompatible { get; }

        /// <summary>
        /// The Font Gylph collection as Lines instead of rows
        /// only the CharMap characters are expected. There could be duplicates
        /// </summary>
        /// <param name="c">character</param>
        /// <returns>Yaff strings one character per pixel</returns>
        List<string> GlyphLines(char c);

        // properties for rendering proportial fonts
        // ------------------------------------------

        /// <summary>
        /// Bearing is the amount of space on left and right
        /// </summary>
        /// <param name="c">character</param>
        /// <returns>tuple</returns>
        (int lb, int rb) GetBearing(char c);
        
        /// <summary>
        /// get the exact width of the current character without bearing
        /// </summary>
        /// <param name="c">character</param>
        /// <returns>int</returns>
        int GetWidth(char c);

        /// <summary>
        /// gets the exact height from the gylphs
        /// </summary>
        /// <param name="c">character</param>
        /// <returns>int</returns>
        int GetHeight(char c);
    }

    /// <summary>
    /// Proportional or Fixed Yaff Font
    /// </summary>
    public enum YaffFontType
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Proportional - not all the same width
        /// </summary>
        Proportional = 1,

        /// <summary>
        /// Fixed - all characters are the same width - like standard IFont
        /// </summary>
        Fixed = 2
    }

    /// <summary>
    /// Constants for reading and using Yaff Fonts
    /// </summary>
    public static class YaffConst
    {
        /// <summary>
        /// represents an on pixel
        /// </summary>
        public static char ink = '@';

        /// <summary>
        /// represents an off pixel
        /// </summary>
        public static char paper = '.';

        /// <summary>
        /// a shortcut for all black character
        /// </summary>
        public static char empty = '-';

        /// <summary>
        /// Valid Gyph characters
        /// </summary>
        public static string glyphchars = new string(new char[] { ink, paper, empty });

        /// <summary>
        /// standard seperator
        /// </summary>
        public static char separator = ':';

        /// <summary>
        /// standard comment indicator
        /// </summary>
        public static readonly char comment = '#';

        /// <summary>
        /// definition of whitespace (tabs and blanks)
        /// </summary>
        public static char[] whitespace = { ' ', '\t' };
    }
}
