using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;

namespace Meadow.Foundation.FeatherWings
{
    /// <summary>
    /// Represents an Adafruit Led Matrix 8x16 feather wing (HT16K33)
    /// </summary>
    public class LedMatrix8x16Wing : IGraphicsDisplay
    {
        private Ht16k33 ht16k33;

        public LedMatrix8x16Wing(II2cBus i2cBus, byte address = (byte)Ht16k33.Addresses.Default)
        {
            ht16k33 = new Ht16k33(i2cBus, address);
        }

        public ColorType ColorMode => ColorType.Format1bpp;

        public int Width => 8;

        public int Height => 16;

        public bool IgnoreOutOfBoundsPixels { get; set; }

        public void Clear(bool updateDisplay = false)
        {
            ht16k33.ClearDisplay();
        }

        public void Fill(Color fillColor, bool updateDisplay = false)
        {
            Fill(0, 0, Width, Height, fillColor);

            if (updateDisplay) Show();
        }

        public void Fill(int x, int y, int width, int height, Color fillColor)
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

        public void DrawBuffer(int x, int y, IDisplayBuffer displayBuffer)
        {
            throw new System.NotImplementedException();
        }

        public void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color1bpp);
        }

        public void DrawPixel(int x, int y, bool colored)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
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

        public void InvertPixel(int x, int y)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
            }

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

        public void Show(int left, int top, int right, int bottom)
        {
            //ToDo - should be possible - check UpdateDisplay and adjust starting address
            Show();
        }
    }
}