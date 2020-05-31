using ICs.IOExpanders;
using Meadow.Foundation.Displays;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.FeatherWings
{
    /// <summary>
    /// Represents an Adafruit CharliePlex 15x7 feather wing
    /// </summary>
    public class CharlieWing : DisplayBase
    {
        Color _pen;
        public enum I2cAddress : byte
        {
            Adddress0x74 = 0x74,
            Adddress0x77 = 0x77
        }
        

        public override DisplayColorMode ColorMode => DisplayColorMode.Format1bpp;

        public override uint Width => 15;

        public override uint Height => 7;

        public byte Frame { get; set; }

        public byte Brightness { get; set; }

        protected readonly IS31FL3731 _iS31FL3731;

        public CharlieWing(II2cBus i2cBus, I2cAddress address = I2cAddress.Adddress0x74)
        {
            Brightness = 255;
            _pen = Color.White;
            _iS31FL3731 = new IS31FL3731(i2cBus, (byte)address);
            _iS31FL3731.Initialize();

            for (byte i = 0; i <= 7; i++)
            {
                _iS31FL3731.SetLedState(i, true);
                _iS31FL3731.Clear(i);
            }
        }

        public override void Clear(bool updateDisplay = false)
        {
            _iS31FL3731.Clear(Frame);
        }

        public override void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, BitmapMode bitmapMode)
        {
            throw new NotImplementedException();
        }

        public override void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, Color color)
        {
            throw new NotImplementedException();
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color, Brightness);
        }

        public virtual void DrawPixel(int x, int y, Color color, byte brightness)
        {
            int temp;

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
            temp = x;
            x = y;
            y = temp;

            if (color == Color.Black)
                _iS31FL3731.SetLedPwm(Frame, (byte)(x + y * 16), 0);
            else
                _iS31FL3731.SetLedPwm(Frame, (byte)(x + y * 16), brightness);
        }

        public override void DrawPixel(int x, int y, bool colored)
        {
            if(colored)
                DrawPixel(x, y, _pen);
            else
                DrawPixel(x, y, Color.Black);
        }

        public override void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, _pen);
        }

        public virtual void DrawPixel(int x, int y, byte brightness)
        {
            DrawPixel(x, y, _pen, brightness);
        }

        public override void SetPenColor(Color pen)
        {
            _pen = pen;
        }

        public override void Show()
        {
            _iS31FL3731.DisplayFrame(Frame);
        }

        public virtual void Show(byte frame)
        {
            _iS31FL3731.DisplayFrame(frame);
        }
    }
}
