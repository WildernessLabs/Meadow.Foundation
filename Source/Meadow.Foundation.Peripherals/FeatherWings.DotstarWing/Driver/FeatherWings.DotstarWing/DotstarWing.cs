using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using static Meadow.Foundation.Leds.Apa102;

namespace Meadow.Foundation.FeatherWings
{
    /// <summary>
    /// Represents Adafruit's Dotstar feather wing 12x6
    /// </summary>
    public class DotstarWing : IGraphicsDisplay
    {
        Apa102 ledMatrix;

        public float Brightness
        {
            get => ledMatrix.Brightness;
            set => ledMatrix.Brightness = value;  
        }

        public DotstarWing(ISpiBus spiBus) : this(spiBus, 72)
        {
        }

        public DotstarWing(ISpiBus spiBus, int numberOfLeds, PixelOrder pixelOrder = PixelOrder.BGR)
        {
            ledMatrix = new Apa102(spiBus, numberOfLeds, pixelOrder);
        }

        public ColorType ColorMode => ColorType.Format12bppRgb444;

        public int Width => 12;

        public int Height => 6;

        public bool IgnoreOutOfBoundsPixels
        {
            get => ledMatrix.IgnoreOutOfBoundsPixels;
            set => ledMatrix.IgnoreOutOfBoundsPixels = value;
        }

        public void Clear(bool updateDisplay = false)
        {
            ledMatrix.Clear(updateDisplay);
        }

        public void DrawPixel(int x, int y, Color color)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
            }

            int minor = x;
            int major = y;
            int majorScale;

            major = Height - 1 - major;
            majorScale = Width;

            int pixelOffset = (major * majorScale) + minor;

            if (pixelOffset >= 0 && pixelOffset < Height * Width)
            {
                ledMatrix.SetLed(pixelOffset, color);
            }
        } 

        public void DrawPixel(int x, int y, bool colored)
        {
            DrawPixel(x, y, colored ? Color.White : Color.Black);
        }

        public void Show()
        {
            ledMatrix.Show();
        }

        public void Show(int left, int top, int right, int bottom)
        {
            ledMatrix.Show(left, top, right, bottom);
        }

        public void InvertPixel(int x, int y)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
            }

            ledMatrix.InvertPixel(x, y);
        }

        public void Fill(Color fillColor, bool updateDisplay = false)
        {
            ledMatrix.Fill(fillColor, updateDisplay);
        }

        public void Fill(int x, int y, int width, int height, Color fillColor)
        {
            ledMatrix.Fill(x, y, width, height, fillColor);
        }

        public void DrawBuffer(int x, int y, IPixelBuffer displayBuffer)
        {
            ledMatrix.DrawBuffer(x, y, displayBuffer);
        }
    }
}