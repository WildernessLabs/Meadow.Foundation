using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace Meadow.Foundation.Leds
{
    public partial class Apa102 : IGraphicsDisplay
    {
        public ColorType ColorMode => ColorType.Format24bppRgb888;

        public int Width => width;

        readonly int width;

        public int Height => height;

        public bool IgnoreOutOfBoundsPixels { get; set; }

        readonly int height;

        public Apa102(ISpiBus spiBus,
                     PixelOrder pixelOrder = PixelOrder.BGR,
                     bool autoWrite = false,
                     int width = 1,
                     int height = 1) : this(spiBus, width * height, pixelOrder, autoWrite)
        {
            this.width = width;
            this.height = height;
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

        public void DrawPixel(int x, int y, Color color)
        {
            if(IgnoreOutOfBoundsPixels)
            {
                if(x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
            }

            SetLed(GetIndexForCoordinate(x, y), color);
    
        }

        public void DrawPixel(int x, int y, bool colored)
        {
            DrawPixel(0, 0, colored ? Color.White : Color.Black);
        }

        public void InvertPixel(int x, int y)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
            }

            var index = 3 * GetIndexForCoordinate(x, y);

            buffer[index] ^= 0xFF;
            buffer[index + 1] ^= 0xFF;
            buffer[index + 2] ^= 0xFF;
        }
    }
}