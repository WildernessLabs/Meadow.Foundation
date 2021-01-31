using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Tft
{
    public class Hx8357d : TftSpiBase
    {
        public override DisplayColorMode DefautColorMode => DisplayColorMode.Format16bppRgb565;

        public Hx8357d(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width = 320, int height = 480, DisplayColorMode displayColorMode = DisplayColorMode.Format16bppRgb565)
            : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height, displayColorMode)
        {
            Initialize();

            SetRotation(Rotation.Normal);
        }

        public override bool IsColorModeSupported(DisplayColorMode mode)
        {
            return mode == DisplayColorMode.Format16bppRgb565;
        }

        protected override void Initialize()
        {
            SendCommand(HX8357_SWRESET);
            Thread.Sleep(10);   
            SendCommand(HX8357D_SETC);
            SendData(0xFF);
            SendData(0x83);
            SendData(0x57);
            SendData(0xFF);
            Thread.Sleep(300);

            // setRGB which also enables SDO
            SendCommand(HX8357_SETRGB);
            SendData(0x80);  //enable SDO pin!
            SendData(0x00);
            SendData(0x06);
            SendData(0x06);

            SendCommand(HX8357D_SETCOM);
            SendData(0x25);  // -1.52V

            SendCommand(HX8357_SETOSC);
            SendData(0x68);  // Normal mode 70Hz, Idle mode 55 Hz

            SendCommand(HX8357_SETPANEL); //Set Panel
            SendData(0x05);  // BGR, Gate direction swapped

            SendCommand(HX8357_SETPWR1);
            SendData(0x00);  // Not deep standby
            SendData(0x15);  //BT
            SendData(0x1C);  //VSPR
            SendData(0x1C);  //VSNR
            SendData(0x83);  //AP
            SendData(0xAA);  //FS

            SendCommand(HX8357D_SETSTBA);
            SendData(0x50);  //OPON normal
            SendData(0x50);  //OPON idle
            SendData(0x01);  //STBA
            SendData(0x3C);  //STBA
            SendData(0x1E);  //STBA
            SendData(0x08);  //GEN

            SendCommand(HX8357D_SETCYC);
            SendData(0x02);  //NW 0x02
            SendData(0x40);  //RTN
            SendData(0x00);  //DIV
            SendData(0x2A);  //DUM
            SendData(0x2A);  //DUM
            SendData(0x0D);  //GDON
            SendData(0x78);  //GDOFF

            SendCommand(HX8357D_SETGAMMA);
            SendData(0x02);
            SendData(0x0A);
            SendData(0x11);
            SendData(0x1d);
            SendData(0x23);
            SendData(0x35);
            SendData(0x41);
            SendData(0x4b);
            SendData(0x4b);
            SendData(0x42);
            SendData(0x3A);
            SendData(0x27);
            SendData(0x1B);
            SendData(0x08);
            SendData(0x09);
            SendData(0x03);
            SendData(0x02);
            SendData(0x0A);
            SendData(0x11);
            SendData(0x1d);
            SendData(0x23);
            SendData(0x35);
            SendData(0x41);
            SendData(0x4b);
            SendData(0x4b);
            SendData(0x42);
            SendData(0x3A);
            SendData(0x27);
            SendData(0x1B);
            SendData(0x08);
            SendData(0x09);
            SendData(0x03);
            SendData(0x00);
            SendData(0x01);

            SendCommand(COLOR_MODE);
            SendData(0x55); // 16 bit

            SendCommand(MADCTL);
            SendData(0xC0);

            SendCommand(HX8357_TEON);  // TE off
            SendData(0x00);

            SendCommand(HX8357_TEARLINE);  // tear line
            SendData(0x00);
            SendData(0x02);

            SendCommand(HX8357_SLPOUT);  //Exit Sleep
            Thread.Sleep(150);

            SendCommand(HX8357_DISPON);  // display on
            Thread.Sleep(50);
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
                    SendData(MADCTL_MX | MADCTL_MY | MADCTL_RGB);
                    break;
                case Rotation.Rotate_90:
                    SendData(MADCTL_MY | MADCTL_MV | MADCTL_RGB);
                    break;
                case Rotation.Rotate_180:
                    SendData(MADCTL_RGB);
                    break;
                case Rotation.Rotate_270:
                    SendData(MADCTL_MX | MADCTL_MV | MADCTL_RGB);
                    break;
            }
        }

        protected const byte HX8357_NOP = 0x00;
        protected const byte HX8357_SWRESET = 0x01;
        protected const byte HX8357_RDDID = 0x04;
        protected const byte HX8357_RDDST = 0x09;

        protected const byte HX8357_RDPOWMODE = 0x0A;
        protected const byte HX8357_RDMADCTL = 0x0B;
        protected const byte HX8357_RDCOLMOD = 0x0C;
        protected const byte HX8357_RDDIM = 0x0D;
        protected const byte HX8357_RDDSDR = 0x0F;

        protected const byte HX8357_SLPIN = 0x10;
        protected const byte HX8357_SLPOUT = 0x11;

        protected const byte HX8357_INVOFF = 0x20;
        protected const byte HX8357_INVON = 0x21;
        protected const byte HX8357_DISPOFF = 0x28;
        protected const byte HX8357_DISPON = 0x29;

        protected const byte HX8357_TEON = 0x35;
        protected const byte HX8357_TEARLINE = 0x44;

        protected const byte HX8357_SETOSC = 0xB0;
        protected const byte HX8357_SETPWR1 = 0xB1;
        protected const byte HX8357_SETRGB = 0xB3;
        protected const byte HX8357D_SETCOM = 0xB6;

        protected const byte HX8357D_SETCYC = 0xB4;
        protected const byte HX8357D_SETC = 0xB9;

        protected const byte HX8357D_SETSTBA = 0xC0;

        protected const byte HX8357_SETPANEL = 0xCC;

        protected const byte HX8357D_SETGAMMA = 0xE0;
    }
}