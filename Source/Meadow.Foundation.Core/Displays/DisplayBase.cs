using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Abstract hardware display class 
    /// </summary>
    public abstract class DisplayBase : IGraphicsDisplay
    {
        /// <summary>
        /// The ColorType for the current display
        /// </summary>
        public abstract ColorType ColorMode { get; }

        /// <summary>
        /// Width of the display in pixels
        /// </summary>
        public abstract int Width { get; }

        /// <summary>
        /// Height of the display in pixels
        /// </summary>
        public abstract int Height { get; }

        /// <summary>
        /// Indicate of the hardware driver should ignore out of bounds pixels
        /// or if the driver should generate an exception.
        /// </summary>
        public bool IgnoreOutOfBoundsPixels { get; set; }

        /// <summary>
        /// Transfer the contents of the buffer to the display.
        /// </summary>
        public abstract void Show();

        /// <summary>
        /// Transfer part of the contents of the buffer to the display
        /// bounded by left, top, right and bottom
        /// </summary>
        public abstract void Show(int left, int top, int right, int bottom);

        /// <summary>
        /// Clear the display.
        /// </summary>
        /// <param name="updateDisplay">Update the dipslay once the buffer has been cleared when true.</param>
        public abstract void Clear(bool updateDisplay = false);

        /// <summary>
        /// Clear the display.
        /// </summary>
        /// <param name="fillColor">The color used to fill the display buffer.</param>
        /// <param name="updateDisplay">Update the dipslay once the buffer has been cleared when true.</param>
        public abstract void Fill(Color fillColor, bool updateDisplay = false);

        /// <summary>
        /// Clear the display.
        /// </summary>
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
        /// Draw a single pixel at the specified color
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

        public abstract void DrawBuffer(int x, int y, IDisplayBuffer displayBuffer);
    }
}