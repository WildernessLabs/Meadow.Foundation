using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Tft
{
    public class Gc9a01 : TftSpiBase
    {
        public override DisplayColorMode DefautColorMode => DisplayColorMode.Format16bppRgb565;

        public Gc9a01(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, 240, 240, DisplayColorMode.Format16bppRgb565)
        {
            Initialize();

            SetRotation(Rotation.Normal);
        }
            
        protected override void Initialize()
        {
            SendCommand(0xEF);
            SendCommand(0xEB);
            SendData(0x14);

            SendCommand(0xFE);
            SendCommand(0xEF);

            SendCommand(0xEB);
            SendData(0x14);

            SendCommand(0x84);
            SendData(0x40);

            SendCommand(0x85);
            SendData(0xFF);

            SendCommand(0x86);
            SendData(0xFF);

            SendCommand(0x87);
            SendData(0xFF);

            SendCommand(0x88);
            SendData(0x0A);

            SendCommand(0x89);
            SendData(0x21);

            SendCommand(0x8A);
            SendData(0x00);

            SendCommand(0x8B);
            SendData(0x80);

            SendCommand(0x8C);
            SendData(0x01);

            SendCommand(0x8D);
            SendData(0x01);

            SendCommand(0x8E);
            SendData(0xFF);

            SendCommand(0x8F);
            SendData(0xFF);

            SendCommand(0xB6);
            SendData(0x00);
            SendData(0x20);

            SendCommand(0x3A);
            SendData(0x05);

            SendCommand(0x90);
            SendData(0x08);
            SendData(0x08);
            SendData(0x08);
            SendData(0x08);

            SendCommand(0xBD);
            SendData(0x06);

            SendCommand(0xBC);
            SendData(0x00);

            SendCommand(0xFF);
            SendData(0x60);
            SendData(0x01);
            SendData(0x04);

            SendCommand(0xC3);
            SendData(0x13);
            SendCommand(0xC4);
            SendData(0x13);

            SendCommand(0xC9);
            SendData(0x22);

            SendCommand(0xBE);
            SendData(0x11);

            SendCommand(0xE1);
            SendData(0x10);
            SendData(0x0E);

            SendCommand(0xDF);
            SendData(0x21);
            SendData(0x0C);
            SendData(0x02);

            SendCommand(0xF0);
            SendData(0x45);
            SendData(0x09);
            SendData(0x08);
            SendData(0x08);
            SendData(0x26);
            SendData(0x2A);

            SendCommand(0xF1);
            SendData(0x43);
            SendData(0x70);
            SendData(0x72);
            SendData(0x36);
            SendData(0x37);
            SendData(0x6F);

            SendCommand(0xF2);
            SendData(0x45);
            SendData(0x09);
            SendData(0x08);
            SendData(0x08);
            SendData(0x26);
            SendData(0x2A);

            SendCommand(0xF3);
            SendData(0x43);
            SendData(0x70);
            SendData(0x72);
            SendData(0x36);
            SendData(0x37);
            SendData(0x6F);

            SendCommand(0xED);
            SendData(0x1B);
            SendData(0x0B);

            SendCommand(0xAE);
            SendData(0x77);

            SendCommand(0xCD);
            SendData(0x63);

            SendCommand(0x70);
            SendData(0x07);
            SendData(0x07);
            SendData(0x04);
            SendData(0x0E);
            SendData(0x0F);
            SendData(0x09);
            SendData(0x07);
            SendData(0x08);
            SendData(0x03);

            SendCommand(0xE8);
            SendData(0x34);

            SendCommand(0x62);
            SendData(0x18);
            SendData(0x0D);
            SendData(0x71);
            SendData(0xED);
            SendData(0x70);
            SendData(0x70);
            SendData(0x18);
            SendData(0x0F);
            SendData(0x71);
            SendData(0xEF);
            SendData(0x70);
            SendData(0x70);

            SendCommand(0x63);
            SendData(0x18);
            SendData(0x11);
            SendData(0x71);
            SendData(0xF1);
            SendData(0x70);
            SendData(0x70);
            SendData(0x18);
            SendData(0x13);
            SendData(0x71);
            SendData(0xF3);
            SendData(0x70);
            SendData(0x70);

            SendCommand(0x64);
            SendData(0x28);
            SendData(0x29);
            SendData(0xF1);
            SendData(0x01);
            SendData(0xF1);
            SendData(0x00);
            SendData(0x07);

            SendCommand(0x66);
            SendData(0x3C);
            SendData(0x00);
            SendData(0xCD);
            SendData(0x67);
            SendData(0x45);
            SendData(0x45);
            SendData(0x10);
            SendData(0x00);
            SendData(0x00);
            SendData(0x00);

            SendCommand(0x67);
            SendData(0x00);
            SendData(0x3C);
            SendData(0x00);
            SendData(0x00);
            SendData(0x00);
            SendData(0x01);
            SendData(0x54);
            SendData(0x10);
            SendData(0x32);
            SendData(0x98);

            SendCommand(0x74);
            SendData(0x10);
            SendData(0x85);
            SendData(0x80);
            SendData(0x00);
            SendData(0x00);
            SendData(0x4E);
            SendData(0x00);

            SendCommand(0x98);
            SendData(0x3e);
            SendData(0x07);

            SendCommand(0x35);

            SendCommand(GC9A01_INVON);

            SendCommand(GC9A01_SLPOUT);
            Thread.Sleep(GC9A01_SLPOUT_DELAY);
            SendCommand(GC9A01_DISPON);
            Thread.Sleep(20);
        }

        public override bool IsColorModeSupported(DisplayColorMode mode)
        {
            if (mode == DisplayColorMode.Format16bppRgb565)
            {
                return true;
            }
            return false;
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
            SendCommand(GC9A01_MADCTL);

            switch (rotation)
            {
                case Rotation.Normal:
                    SendData(MADCTL_MX | MADCTL_MY | MADCTL_BGR);
                    break;
                case Rotation.Rotate_90:
                    SendData(MADCTL_MY | MADCTL_MV | MADCTL_BGR);
                    break;
                case Rotation.Rotate_180:
                    SendData(MADCTL_BGR);
                    break;
                case Rotation.Rotate_270:
                    SendData(MADCTL_MX | MADCTL_MV | MADCTL_BGR);
                    break;
            }
        }

        const byte GC9A01_RST_DELAY = 100;    ///< delay ms wait for reset finish
        const byte GC9A01_SLPIN_DELAY = 120;  ///< delay ms wait for sleep in finish
        const byte GC9A01_SLPOUT_DELAY = 120; ///< delay ms wait for sleep out finish

        const byte GC9A01_SWRESET = 0x01;
        const byte GC9A01_RDDID = 0x04;
        const byte GC9A01_RDDST = 0x09;

        const byte GC9A01_SLPIN = 0x10;
        const byte GC9A01_SLPOUT = 0x11;
        const byte GC9A01_PTLON = 0x12;
        const byte GC9A01_NORON = 0x13;

        const byte GC9A01_INVOFF = 0x20;
        const byte GC9A01_INVON = 0x21;
        const byte GC9A01_DISPOFF = 0x28;
        const byte GC9A01_DISPON = 0x29;

        const byte GC9A01_PTLAR = 0x30;
        const byte GC9A01_COLMOD = 0x3A;
        const byte GC9A01_MADCTL = 0x36;

        const byte GC9A01_RDID1 = 0xDA;
        const byte GC9A01_RDID2 = 0xDB;
        const byte GC9A01_RDID3 = 0xDC;
        const byte GC9A01_RDID4 = 0xDD;
    }
}