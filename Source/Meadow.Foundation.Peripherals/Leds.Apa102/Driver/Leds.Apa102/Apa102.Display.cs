using Meadow.Foundation.Displays;
using Meadow.Hardware;

namespace Meadow.Foundation.Leds
{
    public partial class Apa102 : DisplayBase
    {
        public override DisplayColorMode ColorMode => DisplayColorMode.Format24bppRgb888;

        public override int Width => width;
        int width;

        public override int Height => height;
        int height;

        Color pen = Color.White;

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

        public override void DrawPixel(int x, int y, Color color)
        {
            pen = color;

            SetLed(GetIndexForCoordinate(x, y), color);
    
        }

        public override void DrawPixel(int x, int y, bool colored)
        {
            DrawPixel(0, 0, colored ? Color.White : Color.Black);
        }

        public override void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, pen);
        }

        public override void InvertPixel(int x, int y)
        {
            var index = 3 * GetIndexForCoordinate(x, y);

            buffer[index] ^= 0xFF;
            buffer[index + 1] ^= 0xFF;
            buffer[index + 2] ^= 0xFF;
        }

        public override void SetPenColor(Color pen)
        {
            this.pen = pen;
        }
    }
}