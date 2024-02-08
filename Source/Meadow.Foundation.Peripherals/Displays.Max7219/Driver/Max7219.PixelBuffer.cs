using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays
{
    public partial class Max7219 : IPixelBuffer
    {
        /// <summary>
        /// The bit depth of the display
        /// </summary>
        public int BitDepth => 1;

        /// <summary>
        /// The total bytes used for the display buffer
        /// </summary>
        public int ByteCount => Width * Height >> 3;

        /// <summary>
        /// The backing buffer for the pixel buffer (not implemented)
        /// </summary>
        public byte[] Buffer => throw new System.NotImplementedException();

        /// <summary>
        /// Fill the display with a normalized color to on/off
        /// </summary>
        /// <param name="color">The color to fill</param>
        public void Fill(Color color)
        {
            Fill(color, false);
        }

        /// <summary>
        /// Get the Color of the pixel at a location (not implemented)
        /// </summary>
        /// <param name="x">The x position in pixels</param>
        /// <param name="y">The y position in pixels</param>
        /// <returns>The pixel color</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Color GetPixel(int x, int y)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Set a pixel at a specific location
        /// </summary>
        /// <param name="x">The x position in pixels</param>
        /// <param name="y">The y position in pixels</param>
        /// <param name="color">The pixel color normalized to on/off</param>
        public void SetPixel(int x, int y, Color color) => DrawPixel(x, y, color);
    }
}
