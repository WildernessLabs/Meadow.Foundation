using Meadow.Foundation.Displays;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;

namespace Meadow.Foundation.FeatherWings
{
    /// <summary>
    /// Represents an Adafruit Led Matrix 8x16 feather wing (HT16K33)
    /// </summary>
    public class LedMatrix8x16Wing : DisplayBase
    {
        public const byte DEFAULT_ADDRESS = 0x70;

        private Ht16k33 ht16k33;

        public LedMatrix8x16Wing(II2cBus i2cBus, byte address = DEFAULT_ADDRESS)
        {
            ht16k33 = new Ht16k33(i2cBus, address);
        }

        public override DisplayColorMode ColorMode => DisplayColorMode.Format1bpp;

        public override int Width => 8;

        public override int Height => 16;

        public override void Clear(bool updateDisplay = false)
        {
            ht16k33.ClearDisplay();
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color1bpp);
        }

        public override void DrawPixel(int x, int y, bool colored)
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

        public override void InvertPixel(int x, int y)
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

        public override void Show()
        {
            ht16k33.UpdateDisplay();
        }

        public override void Show(int left, int top, int right, int bottom)
        {
            //ToDo - should be possible - check UpdateDisplay and adjust starting address
            Show();
        }
    }
}