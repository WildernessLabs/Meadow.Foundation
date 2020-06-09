﻿using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Abstract hardware display class 
    /// </summary>
    public abstract class DisplayBase : IDisplay
    {
        /// <summary>
        /// Mode for copying 1 bit bitmaps
        /// </summary>
        public enum BitmapMode
        {
            And,
            Or,
            XOr,
            Copy
        };

        /// <summary>
        /// Enum for Display color mode, defines bit depth and RGB order
        /// </summary>
        public enum DisplayColorMode
        {
            Format1bpp, //1306 and single color ePaper
            Format2bpp, //for 2 color ePaper
            Format8bppMonochome,
            Format12bppRgb444, //TFT in 12 bit mode
            Format16bppRgb555, //not used
            Format16bppRgb565, //TFT in 16 bit mode
            Format18bppRgb666, //TFT in 18 bit mode
            Format24bppRgb888  //not currently used
        }

        /// <summary>
        /// The DisplayColorMode for the current display
        /// </summary>
        public abstract DisplayColorMode ColorMode { get; }

        /// <summary>
        /// Width of the display in pixels
        /// </summary>
        public abstract uint Width { get; }

        /// <summary>
        /// Height of the display in pixels
        /// </summary>
        public abstract uint Height { get; }

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
        /// Clear the display.
        /// </summary>
        /// <param name="updateDisplay">Update the dipslay once the buffer has been cleared when true.</param>
        public abstract void Clear(bool updateDisplay = false);

        public abstract void DrawPixel(int x, int y, Color color);

        /// <summary>
        /// Draw a single pixel at the specified color
        /// For performance, set the pen and then use the overload without a color value
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colored"></param>
        public abstract void DrawPixel(int x, int y, bool colored);

        /// <summary>
        /// Draw a single pixel using the pen color
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public abstract void DrawPixel(int x, int y); 

        /// <summary>
        /// Set the pen color
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colored"></param>
        public abstract void SetPenColor(Color pen);
    }
}