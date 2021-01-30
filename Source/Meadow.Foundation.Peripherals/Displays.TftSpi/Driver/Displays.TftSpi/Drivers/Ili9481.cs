using System;
using System.Threading;
using Meadow.Foundation.Displays.Tft;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Tft
{
    public class Ili9481 : TftSpiBase
    {
        public override DisplayColorMode DefautColorMode => DisplayColorMode.Format12bppRgb444;

        public Ili9481(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width = 320, int height = 480, DisplayColorMode displayColorMode = DisplayColorMode.Format12bppRgb444) 
            : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height, displayColorMode)
        {
            Initialize();

            SetRotation(Rotation.Normal);
        }

        protected override void Initialize()
        {
            SendCommand(TFT_SLPOUT);
            Thread.Sleep(20);

            SendCommand(0xD0);
            SendData(0x07);
            SendData(0x42);
            SendData(0x18);

            SendCommand(0xD1);
            SendData(0x00);
            SendData(0x07);
            SendData(0x10);

            SendCommand(0xD2);
            SendData(0x01);
            SendData(0x02);

            SendCommand(0xC0);
            SendData(0x10);
            SendData(0x3B);
            SendData(0x00);
            SendData(0x02);
            SendData(0x11);

            SendCommand(0xC5);
            SendData(0x03);

            SendCommand(0xC8);
            SendData(0x00);
            SendData(0x32);
            SendData(0x36);
            SendData(0x45);
            SendData(0x06);
            SendData(0x16);
            SendData(0x37);
            SendData(0x75);
            SendData(0x77);
            SendData(0x54);
            SendData(0x0C);
            SendData(0x00);

            SendCommand(MADCTL);
            SendData(0x0A);

            SendCommand(COLOR_MODE);
            if (ColorMode == DisplayColorMode.Format16bppRgb565)
            {
                SendData(0x55);
            }
            else
            {
                SendData(0x53);
            }

            SendCommand((byte)LcdCommand.CASET);
            SendData(0x00);
            SendData(0x00);
            SendData(0x01);
            SendData(0x3F);

            SendCommand((byte)LcdCommand.RASET);
            SendData(0x00);
            SendData(0x00);
            SendData(0x01);
            SendData(0xDF);

            Thread.Sleep(120);
            SendCommand(TFT_DISPON);

            Thread.Sleep(25);
        }

        protected override void SetAddressWindow(int x0, int y0, int x1, int y1)
        {
            SendCommand((byte)LcdCommand.CASET);  // column addr set
            dataCommandPort.State = Data;
            Write((byte)(x0 >> 8));
            Write((byte)(x0 & 0xff));   // XSTART 
            Write((byte)(x1 >> 8));
            Write((byte)(x1 & 0xff));   // XEND

            SendCommand((byte)LcdCommand.RASET);  // row addr set
            dataCommandPort.State = Data;
            Write((byte)(y0 >> 8));
            Write((byte)(y0 & 0xff));    // YSTART
            Write((byte)(y1 >> 8));
            Write((byte)(y1 & 0xff));    // YEND

            SendCommand((byte)LcdCommand.RAMWR);  // write to RAM
        }

        public void SetRotation(Rotation rotation)
        {
            SendCommand(MADCTL);

            switch (rotation)
            {
                case Rotation.Normal:
                    SendData(MADCTL_SS | MADCTL_BGR);
                    break;
                case Rotation.Rotate_90:
                    SendData(MADCTL_MV | MADCTL_BGR);
                    break;
                case Rotation.Rotate_180:
                    SendData(MADCTL_BGR | MADCTL_GS);
                    break;
                case Rotation.Rotate_270:
                    SendData(MADCTL_MV | MADCTL_BGR | MADCTL_SS | MADCTL_GS);
                    break;
            }
        }

        const byte TFT_SWRST = 0x01;
        const byte TFT_SLPIN = 0x10;
        const byte TFT_SLPOUT = 0x11;
        const byte TFT_INVOFF = 0x20;
        const byte TFT_INVON = 0x21;
        const byte TFT_DISPOFF = 0x28;
        const byte TFT_DISPON = 0x29;
    }
}