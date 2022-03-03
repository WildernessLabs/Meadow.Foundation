namespace Meadow.Foundation.Leds
{
    public partial class Apa102
    {
        /// <summary>
        /// Pixel order for APA102 light strip
        /// </summary>
        public enum PixelOrder
        {
            /// <summary>
            /// Red, Green, Blue
            /// </summary>
            RGB,
            /// <summary>
            /// Red, Blue, Green
            /// </summary>
            RBG,
            /// <summary>
            /// Green, Red, Blue
            /// </summary>
            GRB,
            /// <summary>
            /// Green, Blue, Red
            /// </summary>
            GBR,
            /// <summary>
            /// Blue, Red, Green
            /// </summary>
            BRG,
            /// <summary>
            /// Blue, Green, Red
            /// </summary>
            BGR
        }
    }
}
