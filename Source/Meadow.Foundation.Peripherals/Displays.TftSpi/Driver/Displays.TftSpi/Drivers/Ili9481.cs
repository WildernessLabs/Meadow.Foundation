using System;
using System.Threading;
using Meadow.Foundation.Displays.Tft;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Tft
{
    public class Ili9481 : DisplayTftSpiBase
    {
        public Ili9481(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            uint width = 320, uint height = 480) : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height)
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

            SendCommand(TFT_MADCTL);
            SendData(0x0A);

            SendCommand(0x3A);
            SendData(0x55);

            SendCommand(TFT_CASET);
            SendData(0x00);
            SendData(0x00);
            SendData(0x01);
            SendData(0x3F);

            SendCommand(TFT_PASET);
            SendData(0x00);
            SendData(0x00);
            SendData(0x01);
            SendData(0xDF);

            Thread.Sleep(120);
            SendCommand(TFT_DISPON);

            Thread.Sleep(25);
        }

        protected override void SetAddressWindow(uint x0, uint y0, uint x1, uint y1)
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
            SendCommand(TFT_MADCTL);

            switch (rotation)
            {
                case Rotation.Normal:
                    SendData(TFT_MAD_SS | TFT_MAD_BGR);
                    break;
                case Rotation.Rotate_90:
                    SendData(TFT_MAD_MV | TFT_MAD_BGR);
                    break;
                case Rotation.Rotate_180:
                    SendData(TFT_MAD_BGR | TFT_MAD_GS);
                    break;
                case Rotation.Rotate_270:
                    SendData(TFT_MAD_MV | TFT_MAD_BGR | TFT_MAD_SS | TFT_MAD_GS);
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
        const byte TFT_CASET = 0x2A;
        const byte TFT_PASET = 0x2B;
        const byte TFT_RAMWR = 0x2C;
        const byte TFT_RAMRD = 0x2E;
        const byte TFT_MADCTL = 0x36;
        const byte TFT_MAD_MY = 0x80;
        const byte TFT_MAD_MX = 0x40;
        const byte TFT_MAD_MV = 0x20;
        const byte TFT_MAD_ML = 0x10;
        const byte TFT_MAD_RGB = 0x00;
        const byte TFT_MAD_BGR = 0x08;
        const byte TFT_MAD_MH = 0x04;
        const byte TFT_MAD_SS = 0x02;
        const byte TFT_MAD_GS = 0x01;

    }
}
