using Meadow.Foundation.Graphics.Buffers;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// IGraphicsDriver provides a standard interface for working
    /// with display devices.
    /// 
    /// Conceptually, implementing classes should:
    /// 
    /// 1. Have the primary responsibility of passing a buffer of 
    /// "pixel" data to the device, which can be encoded at a variety
    /// of bit depths
    /// 
    /// 2. Use an IPixelBuffer instance to represent the state of the
    /// display as an in-memory object
    /// 
    /// 3. Avoid doing memory/buffer manipulation operations such as
    /// Draw, Fill, Copy, etc. These operations should be the responsibility 
    /// of the IPixelBuffer and existing API methods are for back-compat. They
    /// should wrap the appropriate buffer methods.
    /// 
    /// </summary>
    public interface IGraphicsDriver
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
        /// Provide a buffer that matches this display's color depth, height, and width.
        /// This should be the buffer that is sent to the device when Show is called
        /// </summary>
        public IPixelBuffer PixelBuffer { get; }

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
        /// 
        /// TODO: remove device-specific implementations and
        /// wrap buffer capabilities!
        /// </summary>
        /// <param name="fillColor">The color used to fill the display buffer.</param>
        /// <param name="updateDisplay">Update the dipslay once the buffer has been cleared when true.</param>
        public void Fill(Color fillColor, bool updateDisplay = false);

        /// <summary>
        /// Clear the display.
        /// 
        /// TODO: remove device-specific implementations and
        /// wrap buffer capabilities!
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y postion</param>
        /// <param name="width">width to fill</param>
        /// <param name="height">height to fill</param>
        /// <param name="fillColor">The color used to fill the display buffer.</param>
        public abstract void Fill(int x, int y, int width, int height, Color fillColor);

        /// <summary>
        /// Draw a single pixel at the specified color
        /// 
        /// /// TODO: remove device-specific implementations and
        /// wrap buffer capabilities!
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color">The Meadow.Foundation color of the pixel</param>
        public abstract void DrawPixel(int x, int y, Color color);

        /// <summary>
        /// Enable or disable a single pixel (used for 1bpp displays)
        /// 
        /// TODO: remove device-specific implementations and
        /// wrap buffer capabilities!
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colored"></param>
        public abstract void DrawPixel(int x, int y, bool colored);

        /// <summary>
        /// Invert the color of a single pixel
        /// 
        /// TODO: remove device-specific implementations and
        /// wrap buffer capabilities!
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public abstract void InvertPixel(int x, int y);

        /// <summary>
        /// Draw a buffer to the display
        /// 
        /// TODO: remove device-specific implementations and
        /// wrap buffer capabilities!
        /// </summary>
        public abstract void DrawBuffer(int x, int y, IPixelBuffer displayBuffer);
    }
}
