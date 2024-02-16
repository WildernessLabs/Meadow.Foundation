using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a St7735 TFT color display
    /// </summary>
    public class St7735 : TftSpiBase
    {
        /// <summary>
        /// The default display color mode
        /// </summary>
        public override ColorMode DefaultColorMode => ColorMode.Format12bppRgb444;

        /// <summary>
        /// The color modes supported by the display
        /// </summary>
        public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565 | ColorMode.Format12bppRgb444;

        private readonly DisplayType displayType;

        private byte xOffset;
        private byte yOffset;

        /// <summary>
        /// Create a new St7735 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="displayType">The St7735 display types (displays vary from manufacturer and screen size)</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public St7735(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width, int height,
            DisplayType displayType = DisplayType.ST7735R, ColorMode colorMode = ColorMode.Format12bppRgb444)
            : base(spiBus, chipSelectPin, dcPin, resetPin, width, height, colorMode)
        {
            this.displayType = displayType;

            Initialize();
        }

        /// <summary>
        /// Create a new St7735 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="displayType">The St7735 display types (displays vary from manufacturer and screen size)</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public St7735(ISpiBus spiBus, IDigitalOutputPort chipSelectPort,
                IDigitalOutputPort dataCommandPort, IDigitalOutputPort resetPort,
                int width, int height, DisplayType displayType = DisplayType.ST7735R, ColorMode colorMode = ColorMode.Format12bppRgb444) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, width, height, colorMode)
        {
            this.displayType = displayType;

            Initialize();
        }

        /// <summary>
        /// The ST7735 display type
        /// </summary>
        public enum DisplayType
        {
            /// <summary>
            /// ST7735R
            /// </summary>
            ST7735R,
            /// <summary>
            /// Green tab
            /// </summary>
            ST7735R_GreenTab,
            /// <summary>
            /// Black tab
            /// </summary>
            ST7735R_BlackTab,
            /// <summary>
            /// 128x128 resolution
            /// </summary>
            ST7735R_128x128,
            /// <summary>
            /// 144x144 resolution
            /// </summary>
            ST7735R_144x144,
            /// <summary>
            /// 80x160 resolution
            /// </summary>
            ST7735R_80x160,
            /// <summary>
            /// ST7735B
            /// </summary>
            ST7735B,
        }

        const byte RDDID = 0x04;
        const byte RDDST = 0x09;
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
        const byte PWCTR6 = 0xFC;
        const byte VMCTR1 = 0xC5;
        const byte GMCTRP1 = 0xE0;
        const byte GMCTRN1 = 0xE1;

        private void SendCommand(byte command, byte[] data)
        {
            SendCommand(command);
            SendData(data);
        }

        /// <summary>
        /// Initialize the display
        /// </summary>
        protected override void Initialize()
        {
            if (resetPort != null)
            {
                resetPort.State = true;
                DelayMs(50);
                resetPort.State = false;
                DelayMs(50);
                resetPort.State = true;
                DelayMs(50);
            }
            else
            {
                DelayMs(150); //Not sure if this is needed but can't hurt
            }

            xOffset = yOffset = 0;

            if (displayType == DisplayType.ST7735B)
            {
                Init7735B();
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
                SendCommand((byte)Register.MADCTL, new byte[] { 0xC0 });
                SendCommand(Register.INVOFF);
            }

            dataCommandPort.State = Data;
        }

        private void CommonInit()
        {
            SendCommand(Register.SWRESET);
            DelayMs(150);
            SendCommand(Register.SLPOUT);
            DelayMs(150);
            SendCommand(Register.FRMCTR1);  // frame rate control - normal mode
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

            SendCommand(Register.MADCTL);  // memory access control (directions)
            SendData(0xC8);  // row address/col address, bottom to top refresh

            SendCommand(Register.COLOR_MODE);  // set color mode
            if (ColorMode == ColorMode.Format16bppRgb565)
                SendData(0x05);  // 16-bit color RGB565
            else
                SendData(0x03); //12-bit color RGB444
        }

        private void Init7735B()
        {
            SendCommand(Register.SWRESET);
            DelayMs(150);
            SendCommand(Register.SLPOUT);
            DelayMs(150);

            SendCommand(Register.COLOR_MODE);  // set color mode
            if (ColorMode == ColorMode.Format16bppRgb565)
                SendData(0x05);  // 16-bit color RGB565
            else
                SendData(0x03); //12-bit color RGB444

            SendCommand(Register.FRMCTR1);  // frame rate control - normal mode
            SendData(new byte[] { 0x00, 0x06, 0x03, 10 });// frame rate = fosc / (1 x 2 + 40) * (LINE + 2C + 2D)

            SendCommand(Register.MADCTL);  // memory access control (directions)
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

            SendCommand(LcdCommand.CASET);
            SendData(new byte[]
            {
                0x00, 0x02,             //     XSTART = 2
                0x00, 0x81,             //     XEND = 129
            });

            SendCommand(LcdCommand.RASET);
            SendData(new byte[]
            {
                0x00, 0x02,             //     XSTART = 1
                0x00, 0x81,             //     XEND = 160
            });

            SendCommand(Register.NORON);
            SendCommand(Register.DISPON);

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

            SendCommand(Register.NORON);
            DelayMs(50);
            SendCommand(Register.DISPON);
            DelayMs(10);
        }

        /// <summary>
        /// Set address window for display updates
        /// </summary>
        /// <param name="x0">X start in pixels</param>
        /// <param name="y0">Y start in pixels</param>
        /// <param name="x1">X end in pixels</param>
        /// <param name="y1">Y end in pixels</param>
        protected override void SetAddressWindow(int x0, int y0, int x1, int y1)
        {
            x0 += xOffset;
            y0 += yOffset;

            x1 += xOffset;
            y1 += yOffset;

            base.SetAddressWindow(x0, y0, x1, y1);
        }
    }
}