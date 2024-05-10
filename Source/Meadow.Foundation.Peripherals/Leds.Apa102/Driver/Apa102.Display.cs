using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Leds
{
    public partial class Apa102 : IPixelDisplay
    {
        /// <inheritdoc/>
        public ColorMode ColorMode => ColorMode.Format24bppRgb888;

        /// <inheritdoc/>
        public ColorMode SupportedColorModes => ColorMode.Format24bppRgb888;

        /// <inheritdoc/>
        public int Width => width;
        readonly int width;

        /// <inheritdoc/>
        public int Height => height;
        readonly int height;

        /// <inheritdoc/>
        public IPixelBuffer PixelBuffer => this;

        /// <summary>
        /// Creates a new APA102 object
        /// </summary>
        /// <param name="spiBus">SPI bus</param>
        /// <param name="width">Width of led array</param>
        /// <param name="height">Height of led array</param>
        /// <param name="pixelOrder">Pixel color order</param>
        /// <param name="chipSelectPort">SPI chip select port (optional)</param>
        public Apa102(ISpiBus spiBus,
                     int width,
                     int height,
                     PixelOrder pixelOrder = PixelOrder.BGR,
                     IDigitalOutputPort? chipSelectPort = null) : this(spiBus, width * height, pixelOrder, chipSelectPort)
        {
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Creates a new APA102 object
        /// </summary>
        /// <param name="spiBus">SPI bus</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="width">Width of led array</param>
        /// <param name="height">Height of led array</param>
        /// <param name="pixelOrder">Pixel color order</param>
        public Apa102(
                    ISpiBus spiBus,
                    IPin chipSelectPin,
                    int width,
                    int height,
                    PixelOrder pixelOrder = PixelOrder.BGR)
        : this(spiBus, width, height, pixelOrder, chipSelectPin.CreateDigitalOutputPort())
        {
        }

        private int GetIndexForCoordinate(int x, int y)
        {
            int index = y * width;

            if (y % 2 == 0)
            {
                index += x;
            }
            else
            {
                index += width - x - 1;
            }

            return index;
        }

        /// <summary>
        /// Draw pixel at location
        /// </summary>
        /// <param name="x">x position of pixel</param>
        /// <param name="y">y position of pixel</param>
        /// <param name="color">color of pixel</param>
        public void DrawPixel(int x, int y, Color color)
        {
            SetLed(GetIndexForCoordinate(x, y), color);
        }

        /// <summary>
        /// Draw pixel at a location
        /// Primarily used for monochrome displays, prefer overload that accepts a Color
        /// </summary>
        /// <param name="x">x position of pixel</param>
        /// <param name="y">y position of pixel</param>
        /// <param name="enabled">if true draw white, if false draw black</param>
        public void DrawPixel(int x, int y, bool enabled)
        {
            DrawPixel(x, y, enabled ? Color.White : Color.Black);
        }

        /// <summary>
        /// Invert pixel at location
        /// </summary>
        /// <param name="x">x position of pixel</param>
        /// <param name="y">y position of pixel</param>
        public void InvertPixel(int x, int y)
        {
            var index = 3 * GetIndexForCoordinate(x, y);

            buffer[index] ^= 0xFF;
            buffer[index + 1] ^= 0xFF;
            buffer[index + 2] ^= 0xFF;
        }

        /// <summary>
        /// Fill the entire display buffer with a color
        /// </summary>
        /// <param name="clearColor">color to fill</param>
        /// <param name="updateDisplay">update after fill</param>
        public void Fill(Color clearColor, bool updateDisplay = false)
        {
            byte[] color = { clearColor.R, clearColor.G, clearColor.B };

            for (int i = 0; i < NumberOfLeds; i++)
            {
                SetLed(i, color);
            }

            if (updateDisplay)
            {
                Show();
            }
        }

        /// <summary>
        /// Fill a color in the specified region
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="width">width to fill</param>
        /// <param name="height">height to fill</param>
        /// <param name="fillColor">color to fill</param>
        public void Fill(int x, int y, int width, int height, Color fillColor)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    DrawPixel(i, j, fillColor);
                }
            }
        }

        /// <summary>
        /// Draw a buffer at the specified location
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="displayBuffer">buffer to draw</param>
        public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
        {
            for (int i = 0; i < displayBuffer.Width; i++)
            {
                for (int j = 0; j < displayBuffer.Height; j++)
                {
                    DrawPixel(x + i, j + y, displayBuffer.GetPixel(i, j));
                }
            }
        }
    }
}