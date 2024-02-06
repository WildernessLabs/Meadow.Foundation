using Meadow.Foundation.Graphics.Buffers;
using System;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// Represents a pixel based graphics display
    /// </summary>
    public interface IGraphicsDisplay
    {
        /// <summary>
        /// The currently set color mode for the display
        /// </summary>
        public ColorMode ColorMode { get; }

        /// <summary>
        /// The Color mode supported by the display
        /// </summary>
        public ColorMode SupportedColorModes { get; }

        /// <summary>
        /// Is the color mode supported on this display
        /// </summary>
        /// <param name="mode">The color mode</param>
        public bool IsColorTypeSupported(ColorMode mode)
        {
            return (SupportedColorModes | mode) != 0;
        }

        /// <summary>
        /// Set the color mode for the display
        /// </summary>
        public void SetColorMode(ColorMode mode)
        {
            throw new ArgumentException("Display does not support color mode changes");
        }

        /// <summary>
        /// Width of the display in pixels
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the display in pixels
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Provide a buffer that matches this display's color depth, height, and width
        /// This should be the buffer that is sent to the device when Show is called
        /// </summary>
        public IPixelBuffer PixelBuffer { get; }

        /// <summary>
        /// The color to draw when a pixel is enabled
        /// </summary>
        public Color EnabledColor => Color.White;

        /// <summary>
        /// The color to draw when a pixel is disabled
        /// </summary>
        public Color DisabledColor => Color.Black;

        /// <summary>
        /// Transfer the contents of the buffer to the display
        /// </summary>
        public void Show();

        /// <summary>
        /// Transfer part of the contents of the buffer to the display
        /// bounded by left, top, right and bottom
        /// </summary>
        public void Show(int left, int top, int right, int bottom);

        /// <summary>
        /// Clear the display
        /// </summary>
        /// <param name="updateDisplay">Update the display once the buffer has been cleared when true</param>
        public void Clear(bool updateDisplay = false);

        /// <summary>
        /// Clear the display
        /// </summary>
        /// <param name="fillColor">The color used to fill the display buffer</param>
        /// <param name="updateDisplay">Update the display once the buffer has been cleared when true</param>
        public void Fill(Color fillColor, bool updateDisplay = false);

        /// <summary>
        /// Clear the display
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        /// <param name="width">width to fill in pixels</param>
        /// <param name="height">height to fill in pixels</param>
        /// <param name="fillColor">The color used to fill the display buffer</param>
        public abstract void Fill(int x, int y, int width, int height, Color fillColor);

        /// <summary>
        /// Draw a single pixel at the specified color
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        /// <param name="color">The Meadow Foundation color of the pixel</param>
        public abstract void DrawPixel(int x, int y, Color color);

        /// <summary>
        /// Enable or disable a single pixel (used for 1bpp displays)
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        /// <param name="enabled">On if true, off if false</param>
        public abstract void DrawPixel(int x, int y, bool enabled);

        /// <summary>
        /// Invert the color of a single pixel
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        public abstract void InvertPixel(int x, int y);

        /// <summary>
        /// Draw a buffer to the display
        /// </summary>
        public abstract void WriteBuffer(int x, int y, IPixelBuffer displayBuffer);
    }
}