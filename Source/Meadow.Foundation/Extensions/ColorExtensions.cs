using System.Drawing;
using Meadow.Foundation;

namespace HSBA
{
    public static class ColorExtensions
    {
        public static Color FromAHSV(this Color color, double alpha, double hue, double saturation, double value)
        {
            Converters.HsvToRgb(hue, saturation, value, out double red, out double green, out double blue);

            return Color.FromArgb((int)(255.0 * alpha), (int)(255.0 * red), (int)(255.0 * green), (int)(255.0 * blue));
        }
    }
}
