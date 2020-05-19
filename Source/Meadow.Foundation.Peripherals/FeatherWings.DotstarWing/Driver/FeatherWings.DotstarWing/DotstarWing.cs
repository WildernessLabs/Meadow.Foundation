using Meadow.Foundation.Displays;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using static Meadow.Foundation.Leds.APA102Led;

namespace Meadow.Foundation.FeatherWings
{
    /// <summary>
    /// Represents Adafruit's Dotstar feather wing 12x6
    /// </summary>
    public class DotstarWing : DisplayBase
    {

        Color _penColor;
        APA102Led _ledMatrix;

        public float Brightness
        {
            get => _ledMatrix.Brightness;
            set => _ledMatrix.Brightness = value;  
        }

        public DotstarWing(ISpiBus spiBus, IDigitalOutputPort chipSelect) : this(spiBus,chipSelect,72)
        {
        }

        public DotstarWing(ISpiBus spiBus, IDigitalOutputPort chipSelect, uint numberOfLeds, PixelOrder pixelOrder = PixelOrder.BGR, bool autoWrite = false)
        {
            _penColor = Color.White;
            _ledMatrix = new APA102Led(spiBus, chipSelect, numberOfLeds, pixelOrder, autoWrite);
        }

        public override DisplayColorMode ColorMode => DisplayColorMode.Format12bppRgb444;

        public override uint Width => 12;

        public override uint Height => 6;

        public override void Clear(bool updateDisplay = false)
        {
            _ledMatrix.Clear(updateDisplay);
        }

        public override void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, BitmapMode bitmapMode)
        {

            for (var ordinate = 0; ordinate < height; ordinate++)
            {
                for (var abscissa = 0; abscissa < width; abscissa++)
                {
                    var b = bitmap[(ordinate * width) + abscissa];
                    byte mask = 0x01;
                    for (var pixel = 0; pixel < 8; pixel++)
                    {
                        DrawPixel(x + (8 * abscissa) + pixel, y + ordinate, (b & mask) > 0);
                        mask <<= 1;
                    }
                }
            }
        }

        public override void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, Color color)
        {
            DrawBitmap(x, y, width, height, bitmap, BitmapMode.And);
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
                _ledMatrix.SetLed(pixelOffset, color);
        }

        public override void DrawPixel(int x, int y, bool colored)
        {
            if (colored)
                DrawPixel(x, y, _penColor);
            else
                DrawPixel(x, y, Color.Black);
        }

        public override void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, _penColor);
        }

        public override void SetPenColor(Color pen)
        {
            _penColor = pen;
        }

        public override void Show()
        {
            _ledMatrix.Show();
        }
    }
}
