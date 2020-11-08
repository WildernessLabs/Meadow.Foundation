using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Tft
{
    public class Hx8357d : DisplayTftSpiBase
    {
        public Hx8357d(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            uint width = 320, uint height = 480) : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height)
        {
            Initialize();

            SetRotation(Rotation.Normal);
        }

        protected override void Initialize()
        {
            // setextc
            SendCommand(HX8357D_SETC);
            SendData(0xFF);
            SendData(0x83);
            SendData(0x57);
            Thread.Sleep(300);

            // setRGB which also enables SDO
            SendCommand(HX8357_SETRGB);
            SendData(0x80);  //enable SDO pin!
                              //  SendData(0x00);  //disable SDO pin!
            SendData(0x0);
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

            SendCommand(HX8357_COLMOD);
            SendData(0x55);  // 16 bit

            SendCommand(HX8357_MADCTL);
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
            SendCommand(HX8357_MADCTL);

            switch (rotation)
            {
                case Rotation.Normal:
                    SendData(HX8357_MADCTL_MX | HX8357_MADCTL_MY | HX8357_MADCTL_RGB);
                    break;
                case Rotation.Rotate_90:
                    SendData(HX8357_MADCTL_MY | HX8357_MADCTL_MV | HX8357_MADCTL_RGB);
                    break;
                case Rotation.Rotate_180:
                    SendData(HX8357_MADCTL_RGB);
                    break;
                case Rotation.Rotate_270:
                    SendData(HX8357_MADCTL_MX | HX8357_MADCTL_MV | HX8357_MADCTL_RGB);
                    break;
            }
        }

        const byte HX8357_MADCTL_MY = 0x80;
        const byte HX8357_MADCTL_MX = 0x40;
        const byte HX8357_MADCTL_MV = 0x20;
        const byte HX8357_MADCTL_ML = 0x10;
        const byte HX8357_MADCTL_RGB = 0x00;
        const byte HX8357_MADCTL_BGR = 0X08;

        const byte HX8357_NOP = 0x00;
        const byte HX8357_SWRESET = 0x01;
        const byte HX8357_RDDID = 0x04;
        const byte HX8357_RDDST = 0x09;

        const byte HX8357_RDPOWMODE = 0x0A;
        const byte HX8357_RDMADCTL = 0x0B;
        const byte HX8357_RDCOLMOD = 0x0C;
        const byte HX8357_RDDIM = 0x0D;
        const byte HX8357_RDDSDR = 0x0F;

        const byte HX8357_SLPIN = 0x10;
        const byte HX8357_SLPOUT = 0x11;

        const byte HX8357_INVOFF = 0x20;
        const byte HX8357_INVON = 0x21;
        const byte HX8357_DISPOFF = 0x28;
        const byte HX8357_DISPON = 0x29;

        const byte HX8357_TEON = 0x35;
        const byte HX8357_TEARLINE = 0x44;
        const byte HX8357_MADCTL = 0x36;
        const byte HX8357_COLMOD = 0x3A;

        const byte HX8357_SETOSC = 0xB0;
        const byte HX8357_SETPWR1 = 0xB1;
        const byte HX8357_SETRGB = 0xB3;
        const byte HX8357D_SETCOM = 0xB6;

        const byte HX8357D_SETCYC = 0xB4;
        const byte HX8357D_SETC = 0xB9;

        const byte HX8357D_SETSTBA = 0xC0;

        const byte HX8357_SETPANEL = 0xCC;

        const byte HX8357D_SETGAMMA = 0xE0;
    }
}