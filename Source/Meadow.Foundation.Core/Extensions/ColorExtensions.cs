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

            return new Color((byte)(red * 255), (byte)(green * 255), (byte)(blue * 255), (byte)(alpha * 255));
        }

        /// <summary>
        /// Blend a new color with the current color
        /// </summary>
        /// <param name="color">The source color</param>
        /// <param name="blendColor">The color to blend</param>
        /// <param name="ratio">The ratio of the blend color to source color</param>
        /// <returns>The resulting blended color</returns>
        public static Color Blend(this Color color, Color blendColor, double ratio)
        {
            if (ratio == 0)
            {
                return color;
            }
            if (ratio == 1)
            {
                return blendColor;
            }

            byte r = (byte)(color.R * (1 - ratio) + blendColor.R * ratio);
            byte g = (byte)(color.G * (1 - ratio) + blendColor.G * ratio);
            byte b = (byte)(color.B * (1 - ratio) + blendColor.B * ratio);
            return Color.FromRgb(r, g, b);
        }
    }
}