using Meadow.Foundation.Displays;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using static Meadow.Foundation.Leds.Apa102;

namespace Meadow.Foundation.FeatherWings
{
    /// <summary>
    /// Represents Adafruit's Dotstar feather wing 12x6
    /// </summary>
    public class DotstarWing : IPixelDisplay
    {
        Apa102 ledMatrix;

        public bool IgnoreOutOfBoundsPixels { get; set; } = true;

        public Color PenColor { get; set; } = Color.White;


        public float Brightness
        {
            get => ledMatrix.Brightness;
            set => ledMatrix.Brightness = value;  
        }

        public DotstarWing(ISpiBus spiBus) : this(spiBus,72)
        {
        }

        public DotstarWing(ISpiBus spiBus, int numberOfLeds, PixelOrder pixelOrder = PixelOrder.BGR, bool autoWrite = false)
        {
            ledMatrix = new Apa102(spiBus, numberOfLeds, pixelOrder, autoWrite);
        }

        public DisplayColorMode ColorMode => DisplayColorMode.Format12bppRgb444;

        public int Width => 12;

        public int Height => 6;

        public void Clear(bool updateDisplay = false)
        {
            ledMatrix.Clear(updateDisplay);
        }

        public void DrawPixel(int x, int y, Color color)
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

        public void DrawPixel(int x, int y, bool colored)
        {
            if (colored)
            {
                DrawPixel(x, y, PenColor);
            }
            else
            {
                DrawPixel(x, y, Color.Black);
            }
        }

        public void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, PenColor);
        }

        public void Show()
        {
            ledMatrix.Show();
        }

        public void InvertPixel(int x, int y)
        {
            throw new System.NotImplementedException();
        }
    }
}