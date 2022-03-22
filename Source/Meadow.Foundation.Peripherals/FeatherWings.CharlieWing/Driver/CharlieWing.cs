using System;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;

namespace Meadow.Foundation.FeatherWings
{
    /// <summary>
    /// Represents an Adafruit CharliePlex 15x7 feather wing
    /// </summary>
    public class CharlieWing : IGraphicsDisplay
    {
        /// <summary>
        /// Color mode of display
        /// </summary>
        public ColorType ColorMode => ColorType.Format8bppGray;

        /// <summary>
        /// Width of display in pixels
        /// </summary>
        public int Width => 15;

        /// <summary>
        /// Height of display in pixels
        /// </summary>
        public int Height => 7;

        public byte Frame { get; set; }
        
        public bool IgnoreOutOfBoundsPixels { get; set; }

        protected readonly Is31fl3731 iS31FL3731;

        public CharlieWing(II2cBus i2cBus, byte address = (byte)Is31fl3731.Addresses.Default)
        {
            iS31FL3731 = new Is31fl3731(i2cBus, address);
            iS31FL3731.Initialize();

            for (byte i = 0; i <= 7; i++)
            {
                iS31FL3731.SetLedState(i, true);
                iS31FL3731.Clear(i);
            }
        }

        public void Clear(bool updateDisplay = false)
        {
            iS31FL3731.Clear(Frame);
        }

        public void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color8bppGray);
        }

        public virtual void DrawPixel(int x, int y, byte brightness)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
            }

            if (x > 7)
            {
                x = 15 - x;
                y += 8;
            }
            else
            {
                y = 7 - y;
            }

            //Swap
            var temp = x;
            x = y;
            y = temp;
      
            iS31FL3731.SetLedPwm(Frame, (byte)(x + y * 16), brightness);
        }

        public void DrawPixel(int x, int y, bool colored)
        {
            DrawPixel(x, y, colored?Color.White:Color.Black);
        }

        public void InvertPixel(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void Show()
        {
            iS31FL3731.DisplayFrame(Frame);
        }

        public void Show(int left, int top, int right, int bottom)
        {
            Show();
        }

        public virtual void Show(byte frame)
        {   //ToDo
            iS31FL3731.DisplayFrame(Frame);
        }

        public void Fill(Color clearColor, bool updateDisplay = false)
        {
            throw new NotImplementedException();
        }

        public void Fill(int x, int y, int width, int height, Color fillColor)
        {
            throw new NotImplementedException();
        }

        public void DrawBuffer(int x, int y, IDisplayBuffer displayBuffer)
        {
            throw new NotImplementedException();
        }
    }
}