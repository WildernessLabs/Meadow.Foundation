using Meadow.Foundation.Displays;
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
        Color penColor;
        Apa102 ledMatrix;

        public float Brightness
        {
            get => ledMatrix.Brightness;
            set => ledMatrix.Brightness = value;  
        }

        public DotstarWing(ISpiBus spiBus, IDigitalOutputPort chipSelect) : this(spiBus,chipSelect,72)
        {
        }

        public DotstarWing(ISpiBus spiBus, IDigitalOutputPort chipSelect, uint numberOfLeds, PixelOrder pixelOrder = PixelOrder.BGR, bool autoWrite = false)
        {
            penColor = Color.White;
            ledMatrix = new Apa102(spiBus, chipSelect, numberOfLeds, pixelOrder, autoWrite);
        }

        public override DisplayColorMode ColorMode => DisplayColorMode.Format12bppRgb444;

        public override uint Width => 12;

        public override uint Height => 6;

        public override void Clear(bool updateDisplay = false)
        {
            ledMatrix.Clear(updateDisplay);
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            uint minor = (uint)x;
            uint major = (uint)y;
            uint majorScale;

            major = Height - 1 - major;
            majorScale = Width;

            uint pixelOffset = (major * majorScale) + minor;

            if (pixelOffset >= 0 && pixelOffset < Height * Width)
            {
                ledMatrix.SetLed(pixelOffset, color);
            }
        } 

        public override void DrawPixel(int x, int y, bool colored)
        {
            if (colored)
            {
                DrawPixel(x, y, penColor);
            }
            else
            {
                DrawPixel(x, y, Color.Black);
            }
        }

        public override void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, penColor);
        }

        public override void SetPenColor(Color pen)
        {
            penColor = pen;
        }

        public override void Show()
        {
            ledMatrix.Show();
        }
    }
}