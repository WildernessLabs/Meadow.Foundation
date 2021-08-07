using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Abstract hardware display class 
    /// </summary>
    public interface IPixelDisplay : IDisplay
    {
        /// <summary>
        /// The DisplayColorMode for the current display
        /// </summary>
        public DisplayColorMode ColorMode { get; }

        /// <summary>
        /// Width of the display in pixels
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the display in pixels
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Indicate of the hardware driver should ignore out of bounds pixels
        /// or if the driver should generate an exception.
        /// </summary>
        public bool IgnoreOutOfBoundsPixels { get; set; }

        /// <summary>
        /// Set the pen color
        /// </summary>
        /// <param name="pen">The Meadow.Foundation.Color currently used for drawing</param>
        public Color PenColor { get; set; }

        /// <summary>
        /// Transfer the contents of the buffer to the display.
        /// </summary>
        public void Show();

        /// <summary>
        /// Clear the display.
        /// </summary>
        /// <param name="updateDisplay">Update the dipslay once the buffer has been cleared when true.</param>
        public void Clear(bool updateDisplay = false);

        /// <summary>
        /// Draw a single pixel at the specified color
        /// For performance, set the pen and then use the overload without a color value
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color">The Meadow.Foundation color of the pixel</param>
        public void DrawPixel(int x, int y, Color color);

        /// <summary>
        /// Draw a single pixel at the specified color
        /// For performance, set the pen and then use the overload without a color value
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colored"></param>
        public void DrawPixel(int x, int y, bool colored);

        /// <summary>
        /// Draw a single pixel using the pen color
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawPixel(int x, int y);

        /// <summary>
        /// Invert the color of a single pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void InvertPixel(int x, int y);

        /// <summary>
        /// Draw a bitmap at given coordinate
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="bitmap">bitmap containing the data to display</param>
        public void DrawBitmap(int x, int y, Bitmap bitmap);
    }
}