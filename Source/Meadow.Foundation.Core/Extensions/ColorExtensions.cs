namespace Meadow.Foundation
{
    /// <summary>
    /// Static class for color extension methods 
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Takes Hue, Saturation and Value and returns a Color object
        /// </summary>
        /// <param name="color"></param>
        /// <param name="alpha"></param>
        /// <param name="hue"></param>
        /// <param name="saturation"></param>
        /// <param name="value"></param>
        /// <returns>A Color object</returns>
        public static Color FromAhsv(this Color color, double alpha, double hue, double saturation, double value)
        {
            Converters.HsvToRgb(hue, saturation, value, out double red, out double green, out double blue);

            return new Color(red, green, blue, alpha);

           // return Color.FromArgb((int)(255.0 * alpha), (int)(255.0 * red), (int)(255.0 * green), (int)(255.0 * blue));
        }
    }
}