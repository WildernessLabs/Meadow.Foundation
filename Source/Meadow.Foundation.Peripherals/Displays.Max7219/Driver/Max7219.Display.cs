using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Max7219 LED matrix driver
    /// </summary>
    public partial class Max7219 : IPixelDisplay
    {
        /// <inheritdoc/>
        public ColorMode ColorMode => ColorMode.Format1bpp;

        /// <inheritdoc/>
        public ColorMode SupportedColorModes => ColorMode.Format1bpp;

        /// <inheritdoc/>
        public int Width => DigitColumns; //digit columns align to pixel columns

        /// <inheritdoc/>
        public int Height => 8 * DigitRows; //each digit takes 8 bits so multiply by 8

        /// <inheritdoc/>
        public IPixelBuffer PixelBuffer => this;

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
        /// Clear the display buffer
        /// </summary>
        public void Clear()
        {
            Clear(false);
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
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        /// <param name="enabled">True = turn on pixel, false = turn off pixel</param>

        public void DrawPixel(int x, int y, bool enabled)
        {
            var index = x % 8;

            var display = (y >> 3) + (x >> 3) * DigitRows;

            if (display > DeviceCount)
            {
                return;
            }

            if (enabled)
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
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        public void InvertPixel(int x, int y)
        {
            var index = x % 8;

            var display = (y >> 3) + (x >> 3) * DigitRows;

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
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        /// <param name="displayBuffer">buffer to draw</param>
        public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
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