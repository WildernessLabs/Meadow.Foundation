using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Tft
{
    public class Ili9486 : TftSpiBase
    {
        public override DisplayColorMode DefautColorMode => DisplayColorMode.Format12bppRgb444;

        public Ili9486(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width = 320, int height = 480, DisplayColorMode displayColorMode = DisplayColorMode.Format12bppRgb444) 
            : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height, displayColorMode)
        {
            Initialize();

            SetRotation(Rotation.Normal);
        }

        protected override void Initialize()
        {
            SendCommand(0x11); // Sleep out, also SW reset
            Thread.Sleep(120);

            SendCommand(COLOR_MODE);
            if (ColorMode == DisplayColorMode.Format16bppRgb565)
                SendData(0x55);
            else
                SendData(0x53);

            SendCommand(0xC2);
            SendData(0x44);

            SendCommand(0xC5);
            SendData(0x00);
            SendData(0x00);
            SendData(0x00);
            SendData(0x00);

            SendCommand(0xE0);
            SendData(0x0F);
            SendData(0x1F);
            SendData(0x1C);
            SendData(0x0C);
            SendData(0x0F);
            SendData(0x08);
            SendData(0x48);
            SendData(0x98);
            SendData(0x37);
            SendData(0x0A);
            SendData(0x13);
            SendData(0x04);
            SendData(0x11);
            SendData(0x0D);
            SendData(0x00);

            SendCommand(0xE1);
            SendData(0x0F);
            SendData(0x32);
            SendData(0x2E);
            SendData(0x0B);
            SendData(0x0D);
            SendData(0x05);
            SendData(0x47);
            SendData(0x75);
            SendData(0x37);
            SendData(0x06);
            SendData(0x10);
            SendData(0x03);
            SendData(0x24);
            SendData(0x20);
            SendData(0x00);

            SendCommand(0x20);                     // display inversion OFF

            SendCommand(0x36);
            SendData(0x48);

            SendCommand(0x29);                     // display on
            Thread.Sleep(150);
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
                    SendData(MADCTL_MX | MADCTL_BGR);
                    break;
                case Rotation.Rotate_90:
                    SendData(MADCTL_MV | MADCTL_BGR);
                    break;
                case Rotation.Rotate_180:
                    SendData(MADCTL_BGR | MADCTL_MY);
                    break;
                case Rotation.Rotate_270:
                    SendData(MADCTL_BGR | MADCTL_MV | MADCTL_MX | MADCTL_MY);
                    break;
            }
        }

        const byte TFT_NOP = 0x00;
        const byte TFT_SWRST = 0x01;
        const byte TFT_SLPIN = 0x10;
        const byte TFT_SLPOUT = 0x11;
        const byte TFT_INVOFF = 0x20;
        const byte TFT_INVON = 0x21;
        const byte TFT_DISPOFF = 0x28;
        const byte TFT_DISPON = 0x29;
    }
}
