using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Leds
{
    public partial class Apa102 : IPixelBuffer
    {
        /// <summary>
        /// The color bit depth of the display
        /// </summary>
        public int BitDepth => 24;

        /// <summary>
        /// The size of the display buffer in bytes
        /// </summary>
        public int ByteCount => NumberOfLeds * 3;

        /// <summary>
        /// The display buffer - not implemented for the Apa102 driver
        /// </summary>
        public byte[] Buffer => throw new System.NotImplementedException();

        /// <summary>
        /// Fill the display buffer with a color
        /// </summary>
        /// <param name="color">The fill color</param>
        public void Fill(Color color) => Fill(color, false);

        /// <summary>
        /// Get the color of a pixel for a given coordinate
        /// </summary>
        /// <param name="x">X position in pixels</param>
        /// <param name="y">Y position in pixels</param>
        /// <returns></returns>
        public Color GetPixel(int x, int y)
        {
            var index = 3 * GetIndexForCoordinate(x, y);

            return new Color(red: buffer[index + pixelOrder[0]],
                             green: buffer[index + pixelOrder[1]],
                             blue: buffer[index + pixelOrder[2]]);
        }

        /// <summary>
        /// Set the color of a pixel for a given coordinate
        /// </summary>
        /// <param name="x">X position in pixels</param>
        /// <param name="y">Y position in pixels</param>
        /// <param name="color">Color of pixel</param>
        public void SetPixel(int x, int y, Color color) => SetLed(GetIndexForCoordinate(x, y), color);
    }
}