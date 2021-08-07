using Meadow.Foundation.Displays;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;

namespace Meadow.Foundation.FeatherWings
{
    /// <summary>
    /// Represents an Adafruit Led Matrix 8x16 feather wing (HT16K33)
    /// </summary>
    public class LedMatrix8x16Wing : IPixelDisplay
    {
        public const byte DEFAULT_ADDRESS = 0x70;

        private Ht16k33 ht16k33;
        public Color PenColor { get; set; } = Color.White;

        public bool IgnoreOutOfBoundsPixels { get; set; } = true;

        public LedMatrix8x16Wing(II2cBus i2cBus, byte address = DEFAULT_ADDRESS)
        {
            ht16k33 = new Ht16k33(i2cBus, address);
        }

        public DisplayColorMode ColorMode => DisplayColorMode.Format1bpp;

        public int Width => 8;

        public int Height => 16;

        public void Clear(bool updateDisplay = false)
        {
            ht16k33.ClearDisplay();
        }

        public void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color == Color.Black ? false : true);
        }

        public void DrawPixel(int x, int y, bool colored)
        {
           if(x >= Width || x < 0 || y >= Height || y < 0)
            {
                return; 
            }

            if (y < 8)
            {
                y *= 2;
            }
            else
            {
                y = (y - 8) * 2 + 1;
            }
            ht16k33.SetLed((byte)(y * Width + x), colored);
        }

        public void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, PenColor == Color.Black ? false : true);
        }

        public void InvertPixel(int x, int y)
        {
            if (y < 8)
            {
                y *= 2;
            }
            else
            {
                y = (y - 8) * 2 + 1;
            }

            ht16k33.ToggleLed((byte)(y * Width + x));
        }

        public void Show()
        {
            ht16k33.UpdateDisplay();
        }
    }
}