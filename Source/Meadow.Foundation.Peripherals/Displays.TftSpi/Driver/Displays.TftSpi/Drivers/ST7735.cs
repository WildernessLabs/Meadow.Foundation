using Meadow.Hardware;
using System.Threading;

namespace Meadow.Foundation.Displays.Tft
{
    public class St7735 : TftSpiBase
    {
        private DisplayType displayType;

        private byte xOffset;
        private byte yOffset;

        public override DisplayColorMode DefautColorMode => DisplayColorMode.Format12bppRgb444;

        public St7735(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width, int height,
            DisplayType displayType = DisplayType.ST7735R, DisplayColorMode displayColorMode = DisplayColorMode.Format12bppRgb444)
            : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height, displayColorMode)
        {
            this.displayType = displayType;

            Initialize();
        }

        public enum DisplayType
        {
            ST7735R,
            ST7735R_GreenTab,
            ST7735R_BlackTab,
            ST7735R_128x128,
            ST7735R_144x144,
            ST7735R_80x160,
            ST7735B, //done
        }

        const byte SWRESET = 0x01;
        const byte RDDID = 0x04;
        const byte RDDST = 0x09;
        const byte SLPIN = 0x10;
        const byte SLPOUT = 0x11;
        const byte PTLON = 0x12;
        const byte NORON = 0x13;
        const byte INVOFF = 0x20;
        const byte INVON = 0x21;
        const byte DISPOFF = 0x28;
        const byte DISPON = 0x29;
        const byte FRMCTR1 = 0xB1;
        const byte FRMCTR2 = 0xB2;
        const byte FRMCTR3 = 0xB3;
        const byte INVCTR = 0xB4;
        const byte DISSET5 = 0xB6;
        const byte PWCTR1 = 0xC0;
        const byte PWCTR2 = 0xC1;
        const byte PWCTR3 = 0xC2;
        const byte PWCTR4 = 0xC3;
        const byte PWCTR5 = 0xC4;
        const byte VMCTR1 = 0xC5;
        const byte RDID1 = 0xDA;
        const byte RDID2 = 0xDB;
        const byte RDID3 = 0xDC;
        const byte RDID4 = 0xDD;
        const byte PWCTR6 = 0xFC;
        const byte GMCTRP1 = 0xE0;
        const byte GMCTRN1 = 0xE1;

        private void SendCommand(byte command, byte[] data)
        {
            SendCommand(command);
            SendData(data);
        }

        protected override void Initialize()
        {
            resetPort.State = true;
            Thread.Sleep(50);
            resetPort.State = false;
            Thread.Sleep(50);
            resetPort.State = true;
            Thread.Sleep(50);

            xOffset = yOffset = 0;

            if (displayType == DisplayType.ST7735B)
            {
                Init7735B();
                SetAddressWindow(0, 0, (width - 1), (height - 1));
                return;
            }

            CommonInit();

            if (displayType == DisplayType.ST7735R_GreenTab)
                Init7735RGreen();
            else if (displayType == DisplayType.ST7735R_144x144)
                Init7735RGreen144x144();
            else if (displayType == DisplayType.ST7735R_80x160)
                Init7735RGreen80x160();
            else
                Init7735RRed();

            Init7735REnd();

            if (displayType == DisplayType.ST7735R_80x160 ||
                displayType == DisplayType.ST7735R_BlackTab)
            {
                SendCommand(MADCTL, new byte[] { 0xC0 });
                SendCommand(INVOFF);
            }

            SetAddressWindow(0, 0, (width - 1), (height - 1));

            dataCommandPort.State = Data;
        }

        private void CommonInit()
        {
            SendCommand(SWRESET);
            DelayMs(150);
            SendCommand(SLPOUT);
            DelayMs(150);
            SendCommand(FRMCTR1);  // frame rate control - normal mode
            SendData(new byte[] { 0x01, 0x2C, 0x2D });// frame rate = fosc / (1 x 2 + 40) * (LINE + 2C + 2D)

            SendCommand(FRMCTR2);  // frame rate control - idle mode
            SendData(0x01);  // frame rate = fosc / (1 x 2 + 40) * (LINE + 2C + 2D)
            SendData(0x2C);
            SendData(0x2D);

            SendCommand(FRMCTR3);  // frame rate control - partial mode
            SendData(0x01); // dot inversion mode
            SendData(0x2C);
            SendData(0x2D);
            SendData(0x01); // line inversion mode
            SendData(0x2C);
            SendData(0x2D);

            SendCommand(INVCTR);  // display inversion control
            SendData(0x07);  // no inversion

            SendCommand(PWCTR1);  // power control
            SendData(0xA2);
            SendData(0x02);      // -4.6V
            SendData(0x84);      // AUTO mode

            SendCommand(PWCTR2);  // power control
            SendData(0xC5);      // VGH25 = 2.4C VGSEL = -10 VGH = 3 * AVDD

            SendCommand(PWCTR3);  // power control
            SendData(0x0A);      // Opamp current small 
            SendData(0x00);      // Boost frequency

            SendCommand(PWCTR4);  // power control
            SendData(0x8A);      // BCLK/2, Opamp current small & Medium low
            SendData(0x2A);

            SendCommand(PWCTR5);  // power control
            SendData(0x8A);
            SendData(0xEE);

            SendCommand(VMCTR1);  // power control
            SendData(0x0E);

            SendCommand(MADCTL);  // memory access control (directions)
            SendData(0xC8);  // row address/col address, bottom to top refresh

            SendCommand(COLOR_MODE);  // set color mode
            if (ColorMode == DisplayColorMode.Format16bppRgb565)
                SendData(0x05);  // 16-bit color RGB565
            else
                SendData(0x03); //12-bit color RGB444
        }

        private void Init7735B()
        {
            SendCommand(SWRESET);
            DelayMs(150);
            SendCommand(SLPOUT);
            DelayMs(150);

            SendCommand(COLOR_MODE);  // set color mode
            if (ColorMode == DisplayColorMode.Format16bppRgb565)
                SendData(0x05);  // 16-bit color RGB565
            else
                SendData(0x03); //12-bit color RGB444

            SendCommand(FRMCTR1);  // frame rate control - normal mode
            SendData(new byte[] { 0x00, 0x06, 0x03, 10 });// frame rate = fosc / (1 x 2 + 40) * (LINE + 2C + 2D)

            SendCommand(MADCTL);  // memory access control (directions)
            SendData(0xC8);  // row address/col address, bottom to top refresh

            SendCommand(DISSET5);
            SendData(new byte[] { 0x15, 0x02 });

            SendCommand(INVCTR);  // display inversion control
            SendData(0x07);  // no inversion

            SendCommand(PWCTR1);  // power control
            dataCommandPort.State = Data;
            Write(0x02);
            Write(0x70);
            Write(10);

            SendCommand(PWCTR2);  // power control
            SendData(0xC5);      // VGH25 = 2.4C VGSEL = -10 VGH = 3 * AVDD

            SendCommand(PWCTR3);  // power control
            dataCommandPort.State = Data;
            Write(0x01);      // Opamp current small 
            Write(0x02);      // Boost frequency

            SendCommand(VMCTR1);  // power control
            SendData(new byte[] { 0x3C, 0x38, 10 });

            SendCommand(PWCTR6);
            SendData(new byte[] { 0x11, 0x15 });

            SendCommand(GMCTRP1);
            SendData(new byte[]
            {
                0x09, 0x16, 0x09, 0x20, 0x21, 0x1B, 0x13, 0x19,
                0x17, 0x15, 0x1E, 0x2B, 0x04, 0x05, 0x02, 0x0E
            });

            SendCommand(GMCTRN1);
            SendData(new byte[]
            {
                0x0B, 0x14, 0x08, 0x1E, 0x22, 0x1D, 0x18, 0x1E,
                0x1B, 0x1A, 0x24, 0x2B, 0x06, 0x06, 0x02, 0x0F,
            });

            SendCommand((byte)LcdCommand.CASET);
            SendData(new byte[]
            {
                0x00, 0x02,             //     XSTART = 2
                0x00, 0x81,             //     XEND = 129
            });

            SendCommand((byte)LcdCommand.RASET);
            SendData(new byte[]
            {
                0x00, 0x02,             //     XSTART = 1
                0x00, 0x81,             //     XEND = 160
            });

            SendCommand(NORON);
            SendCommand(DISPON);

            DelayMs(500);
        }

        private void Init7735RGreen()
        {
            SendCommand((byte)LcdCommand.CASET, new byte[] { 0x00, 0x02, 0x00, 0x7F + 0x02 });
            SendCommand((byte)LcdCommand.RASET, new byte[] { 0x00, 0x01, 0x00, 0x9F + 0x01 });

            xOffset = 1;
            yOffset = 2;
        }

        private void Init7735RRed()
        {
            SendCommand((byte)LcdCommand.CASET, new byte[] { 0x00, 0x00, 0x00, 0x7F });
            SendCommand((byte)LcdCommand.RASET, new byte[] { 0x00, 0x00, 0x00, 0x9F });
        }

        private void Init7735RGreen144x144()
        {
            SendCommand((byte)LcdCommand.CASET, new byte[] { 0x00, 0x00, 0x00, 0x7F });
            SendCommand((byte)LcdCommand.RASET, new byte[] { 0x00, 0x00, 0x00, 0x7F });

            xOffset = 2;
            yOffset = 1;
        }

        private void Init7735RGreen80x160()
        {
            SendCommand((byte)LcdCommand.CASET, new byte[] { 0x00, 0x00, 0x00, 0x7F });
            SendCommand((byte)LcdCommand.RASET, new byte[] { 0x00, 0x00, 0x00, 0x9F });

            xOffset = 26;
            yOffset = 1;
        }

        private void Init7735REnd()
        {
            SendCommand(GMCTRP1);
            SendData(new byte[]
            {
                0x02, 0x1c, 0x07, 0x12, 0x37, 0x32, 0x29, 0x2d,
                0x29, 0x25, 0x2B, 0x39, 0x00, 0x01, 0x03, 0x10,
            });

            SendCommand(GMCTRN1);
            SendData(new byte[]
            {
                0x03, 0x1d, 0x07, 0x06, 0x2E, 0x2C, 0x29, 0x2D,
                0x2E, 0x2E, 0x37, 0x3F, 0x00, 0x00, 0x02, 0x10,
            });

            SendCommand(NORON);
            Thread.Sleep(50);
            SendCommand(DISPON);
            Thread.Sleep(10);
        }

        protected override void SetAddressWindow(int x0, int y0, int x1, int y1)
        {
            x0 += xOffset;
            y0 += yOffset;

            x1 += xOffset;
            y1 += yOffset;

            SendCommand((byte)LcdCommand.CASET);  // column addr set
            dataCommandPort.State = Data;
            Write((byte)(x0 >> 8));
            Write((byte)(x0 & 0xff));   // XSTART 
            Write((byte)(x1 >> 8));
            Write((byte)(x1 & 0xff));   // XEND

            SendCommand((byte)LcdCommand.RASET);  // row addr set
            dataCommandPort.State = Data;
            Write((byte)(y0 >> 8));
            Write((byte)(y0 & 0xff));   // YSTART 
            Write((byte)(y1 >> 8));
            Write((byte)(y1 & 0xff));   // YEND

            SendCommand((byte)LcdCommand.RAMWR);  // write to RAM
        }
    }
}