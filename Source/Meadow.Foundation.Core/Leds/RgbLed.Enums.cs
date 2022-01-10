namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Possible colors on RgbLed
    /// </summary>
    public partial class RgbLed
    {
        /// <summary>
        /// Colors for RGB Led
        /// </summary>
        public enum Colors
        {
            /// <summary>
            /// Red (red LED only)
            /// </summary>
            Red,
            /// <summary>
            /// Green (green LED only)
            /// </summary>
            Green,
            /// <summary>
            /// Blue (blue LED only)
            /// </summary>
            Blue,
            /// <summary>
            /// Yellow (red and green LEDs)
            /// </summary>
            Yellow,
            /// <summary>
            /// Magenta (blue and red LEDs)
            /// </summary>
            Magenta,
            /// <summary>
            /// Cyan (blue and green LEDs)
            /// </summary>
            Cyan,
            /// <summary>
            /// White (red, green and blue LEDs)
            /// </summary>
            White,
            /// <summary>
            /// Black (all LEDs off)
            /// </summary>
            Black,
            /// <summary>
            /// Count of colors
            /// </summary>
            count,
        }
    }
}