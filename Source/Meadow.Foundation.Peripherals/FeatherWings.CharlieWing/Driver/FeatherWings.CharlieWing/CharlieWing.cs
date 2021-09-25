using System;
using Meadow.Foundation.Displays;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;

namespace Meadow.Foundation.FeatherWings
{
    /// <summary>
    /// Represents an Adafruit CharliePlex 15x7 feather wing
    /// </summary>
    public class CharlieWing : DisplayBase
    {
        public const byte DEFAULT_ADDRESS = 0x74;
        public const byte ALTERNATE_ADDRESS = 0x77;

        public override DisplayColorMode ColorMode => DisplayColorMode.Format1bpp;

        public override int Width => 15;

        public override int Height => 7;

        public byte Frame { get; set; }

        public byte Brightness { get; set; }

        protected readonly Is31fl3731 iS31FL3731;

        public CharlieWing(II2cBus i2cBus, byte address = DEFAULT_ADDRESS)
        {
            Brightness = 255;
            iS31FL3731 = new Is31fl3731(i2cBus, (byte)address);
            iS31FL3731.Initialize();

            for (byte i = 0; i <= 7; i++)
            {
                iS31FL3731.SetLedState(i, true);
                iS31FL3731.Clear(i);
            }
        }

        public override void Clear(bool updateDisplay = false)
        {
            iS31FL3731.Clear(Frame);
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color8bppGray);
        }

        public virtual void DrawPixel(int x, int y, byte brightness)
        {
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

        public override void DrawPixel(int x, int y, bool colored)
        {
            DrawPixel(x, y, colored?Color.White:Color.Black);
        }

        public override void InvertPixel(int x, int y)
        {
            throw new NotImplementedException();
        }

        public override void Show()
        {
            iS31FL3731.DisplayFrame(Frame);
        }

        public virtual void Show(byte frame)
        {
            iS31FL3731.DisplayFrame(frame);
        }
    }
}