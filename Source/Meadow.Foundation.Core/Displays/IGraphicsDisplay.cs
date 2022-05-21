using Meadow.Foundation.Graphics.Buffers;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// Represents a pixel based graphics display
    /// </summary>
    public interface IGraphicsDisplay
    {
        /// <summary>
        /// The ColorType for the current display
        /// </summary>
        public ColorType ColorMode { get; }

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
        /// Transfer the contents of the buffer to the display.
        /// </summary>
        public void Show();

        /// <summary>
        /// Transfer part of the contents of the buffer to the display
        /// bounded by left, top, right and bottom
        /// </summary>
        public void Show(int left, int top, int right, int bottom);

        /// <summary>
        /// Clear the display.
        /// </summary>
        /// <param name="updateDisplay">Update the dipslay once the buffer has been cleared when true.</param>
        public void Clear(bool updateDisplay = false);

        /// <summary>
        /// Clear the display.
        /// </summary>
        /// <param name="fillColor">The color used to fill the display buffer.</param>
        /// <param name="updateDisplay">Update the dipslay once the buffer has been cleared when true.</param>
        public void Fill(Color fillColor, bool updateDisplay = false);

        /// <summary>
        /// Clear the display.
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y postion</param>
        /// <param name="width">width to fill</param>
        /// <param name="height">height to fill</param>
        /// <param name="fillColor">The color used to fill the display buffer.</param>
        public abstract void Fill(int x, int y, int width, int height, Color fillColor);

        /// <summary>
        /// Draw a single pixel at the specified color
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color">The Meadow.Foundation color of the pixel</param>
        public abstract void DrawPixel(int x, int y, Color color);

        /// <summary>
        /// Enable or disable a single pixel (used for 1bpp displays)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colored"></param>
        public abstract void DrawPixel(int x, int y, bool colored);

        /// <summary>
        /// Invert the color of a single pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public abstract void InvertPixel(int x, int y);

        /// <summary>
        /// Draw a buffer to the display
        /// </summary>
        public abstract void DrawBuffer(int x, int y, IPixelBuffer displayBuffer);
    }
}
