using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;

namespace Meadow.Foundation.FeatherWings
{
    /// <summary>
    /// Represents an Adafruit Led Matrix 8x16 feather wing (HT16K33)
    /// </summary>
    public class LedMatrix8x16Wing : DisplayBase
    {
        private Ht16k33 ht16k33;

        public LedMatrix8x16Wing(II2cBus i2cBus, byte address = (byte)Ht16k33.Addresses.Default)
        {
            ht16k33 = new Ht16k33(i2cBus, address);
        }

        public override ColorType ColorMode => ColorType.Format1bpp;

        public override int Width => 8;

        public override int Height => 16;

        public override void Clear(bool updateDisplay = false)
        {
            ht16k33.ClearDisplay();
        }

        public override void Fill(Color fillColor, bool updateDisplay = false)
        {
            Fill(0, 0, Width, Height, fillColor);

            if (updateDisplay) Show();
        }

        public override void Fill(int x, int y, int width, int height, Color fillColor)
        {
            bool isColored = fillColor.Color1bpp;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    DrawPixel(i, j, isColored);
                }
            }
        }

        public override void DrawBuffer(int x, int y, IDisplayBuffer displayBuffer)
        {
            throw new System.NotImplementedException();
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