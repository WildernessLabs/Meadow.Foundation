using Meadow.Foundation.Displays;
using Meadow.Hardware;
using System;
using static Meadow.Foundation.Leds.Apa102;

namespace Meadow.Foundation.Leds
{
    public partial class Apa102 : DisplayBase
    {
        public override DisplayColorMode ColorMode => DisplayColorMode.Format24bppRgb888;

        public override int Width => width;
        int width;

        public override int Height => height;
        int height;

        byte[] displayBuffer;
        byte[] readBuffer;

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

        public override void Clear(bool updateDisplay = false)
        {
            Array.Clear(displayBuffer, 0, displayBuffer.Length);
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            pen = color;

            int index = y * width;

            if(y % 2 == 0)
            {
                index += x;
            }
            else
            {
                index += width - x;
            }
            SetLed(index, color);
    
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
            throw new System.NotImplementedException();
        }

        public override void SetPenColor(Color pen)
        {
            this.pen = pen;
        }
    }
}