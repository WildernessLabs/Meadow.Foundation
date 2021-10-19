using Meadow.Foundation.Displays;
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
    public class DotstarWing : DisplayBase
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

        public DotstarWing(ISpiBus spiBus, int numberOfLeds, PixelOrder pixelOrder = PixelOrder.BGR, bool autoWrite = false)
        {
            ledMatrix = new Apa102(spiBus, numberOfLeds, pixelOrder, autoWrite);
        }

        public override ColorType ColorMode => ColorType.Format12bppRgb444;

        public override int Width => 12;

        public override int Height => 6;

        public override void Clear(bool updateDisplay = false)
        {
            ledMatrix.Clear(updateDisplay);
        }

        public override void DrawPixel(int x, int y, Color color)
        {
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

        public override void DrawPixel(int x, int y, bool colored)
        {
            DrawPixel(x, y, colored ? Color.White : Color.Black);
        }

        public override void Show()
        {
            ledMatrix.Show();
        }

        public override void Show(int left, int top, int right, int bottom)
        {
            ledMatrix.Show(left, top, right, bottom);
        }

        public override void InvertPixel(int x, int y)
        {
            ledMatrix.InvertPixel(x, y);
        }

        public override void Clear(Color clearColor, bool updateDisplay = false)
        {
            ledMatrix.Clear(clearColor, updateDisplay);
        }

        public override void DrawBuffer(int x, int y, IDisplayBuffer displayBuffer)
        {
            ledMatrix.DrawBuffer(x, y, displayBuffer);
        }
    }
}