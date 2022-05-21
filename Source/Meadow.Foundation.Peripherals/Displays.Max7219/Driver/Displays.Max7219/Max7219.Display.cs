using System;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Graphics;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Max7219 LED matrix driver
    /// </summary>
    public partial class Max7219 : IGraphicsDriver
    {
        /// <summary>
        /// Color mode of display - 1bpp
        /// </summary>
        public ColorType ColorMode => ColorType.Format1bpp;

        /// <summary>
        /// Width of array of displays in pixels
        /// </summary>
        public int Width => DigitColumns; //digit columns align to pixel columns

        /// <summary>
        /// Height of array of displays in pixels
        /// </summary>
        public int Height => 8 * DigitRows; //each digit takes 8 bits so multiply by 8

        /// <summary>
        /// Will display ignore out of bounds pixels
        /// </summary>
        public bool IgnoreOutOfBoundsPixels { get; set; }

        /// <summary>
        /// This device does not use a pixel buffer, it's methods directly light up LEDs on the device.
        /// </summary>
        public IPixelBuffer PixelBuffer => throw new NotImplementedException("This driver directly interfaces with device and doesn't implement an IPixelBuffer");

        /// <summary>
        /// Partial screen update
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        public void Show(int left, int top, int right, int bottom)
        {
            //ToDo Check if partial updates are possible (although it's pretty fast as is)
            Show();
        }

        /// <summary>
        /// Clears the buffer from the given start to end and flushes
        /// </summary>
        public void Clear(bool updateDisplay = false)
        {
            Clear(0, DeviceCount);

            if (updateDisplay)
            {
                Show();
            }
        }

        /// <summary>
        /// Draw pixel at location
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="color">color of pixel - converted to on/off</param>
        public void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color1bpp);
        }

        /// <summary>
        /// Draw pixel at location
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="colored">Is pixel colored - on/off</param>

        public void DrawPixel(int x, int y, bool colored)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
            }

            var index = x % 8;

            var display = y / 8 + (x / 8) * DigitRows;

            if (display > DeviceCount)
            {
                return;
            }

            if (colored)
            {
                buffer[display, index] = (byte)(buffer[display, index] | (byte)(1 << (y % 8)));
            }
            else
            {
                buffer[display, index] = (byte)(buffer[display, index] & ~(byte)(1 << (y % 8)));
            }
        }

        /// <summary>
        /// Invert pixel at location (toggle on/off)
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        public void InvertPixel(int x, int y)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
            }

            var index = x % 8;

            var display = y / 8 + (x / 8) * DigitRows;

            if (display > DeviceCount)
            {
                return;
            }

            buffer[display, index] = (buffer[display, index] ^= (byte)(1 << y % 8));
        }

        /// <summary>
        /// Fill with color 
        /// </summary>
        /// <param name="fillColor">color - converted to on/off</param>
        /// <param name="updateDisplay">should refresh display</param>
        public void Fill(Color fillColor, bool updateDisplay = false)
        {
            Fill(0, 0, Width, Height, fillColor);

            if (updateDisplay) Show();
        }

        /// <summary>
        /// Fill region with color
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="width">width of region</param>
        /// <param name="height">height of region</param>
        /// <param name="fillColor">color - converted to on/off</param>
        public void Fill(int x, int y, int width, int height, Color fillColor)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0) x = 0;
                if (y < 0) y = 0;
                if (x > width - 1) x = width - 1;
                if (y > height - 1) y = height - 1;
            }

            bool isColored = fillColor.Color1bpp;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    DrawPixel(i + x, j + y, isColored);
                }
            }
        }

        /// <summary>
        /// Draw buffer at location
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="displayBuffer">buffer to draw</param>
        public void DrawBuffer(int x, int y, IPixelBuffer displayBuffer)
        {   //need to refactor to use a proper buffer
            for (int i = 0; i < displayBuffer.Width; i++)
            {
                for (int j = 0; j < displayBuffer.Height; j++)
                {
                    DrawPixel(x + i, j + y, displayBuffer.GetPixel(i, j).Color1bpp);
                }
            }
        }
    }
}