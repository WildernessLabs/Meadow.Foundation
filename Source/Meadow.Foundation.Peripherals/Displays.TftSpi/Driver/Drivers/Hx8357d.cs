using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a Hx8357d TFT color display
    /// </summary>
    public class Hx8357d : TftSpiBase, IRotatableDisplay
    {
        /// <summary>
        /// The display default color mode
        /// </summary>
        public override ColorMode DefaultColorMode => ColorMode.Format16bppRgb565;

        /// <summary>
        /// The color modes supported by the display
        /// </summary>
        public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565;

        /// <summary>
        /// Create a new Hx8357d color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public Hx8357d(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width = 320, int height = 480, ColorMode colorMode = ColorMode.Format16bppRgb565)
            : base(spiBus, chipSelectPin, dcPin, resetPin, width, height, colorMode)
        {
            Initialize();

            SetRotation(RotationType.Normal);
        }

        /// <summary>
        /// Create a new Hx8357d color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public Hx8357d(ISpiBus spiBus, IDigitalOutputPort chipSelectPort,
                IDigitalOutputPort dataCommandPort, IDigitalOutputPort resetPort,
                int width = 320, int height = 480, ColorMode colorMode = ColorMode.Format16bppRgb565) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, width, height, colorMode)
        {
            Initialize();

            SetRotation(RotationType.Normal);
        }

        /// <summary>
        /// Initialize the display
        /// </summary>
        protected override void Initialize()
        {
            SendCommand(Register.SWRESET);
            DelayMs(10);
            SendCommand(HX8357D_SETC);
            SendData(0xFF);
            SendData(0x83);
            SendData(0x57);
            DelayMs(300);

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

            SendCommand(Register.COLOR_MODE);
            SendData(0x55); // 16 bit

            SendCommand(Register.MADCTL);
            SendData(0xC0);

            SendCommand(HX8357_TEON);  // TE off
            SendData(0x00);

            SendCommand(HX8357_TEARLINE);  // tear line
            SendData(0x00);
            SendData(0x02);

            SendCommand(Register.SLPOUT);  //Exit Sleep
            DelayMs(150);

            SendCommand(Register.DISPON);  // display on
            DelayMs(50);
        }

        /// <summary>
        /// Set the display rotation
        /// </summary>
        /// <param name="rotation">The rotation value</param>
        public void SetRotation(RotationType rotation)
        {
            SendCommand(Register.MADCTL);

            switch (Rotation = rotation)
            {
                case RotationType.Normal:
                    SendData((byte)(Register.MADCTL_MX | Register.MADCTL_MY | Register.MADCTL_RGB));
                    break;
                case RotationType._90Degrees:
                    SendData((byte)(Register.MADCTL_MY | Register.MADCTL_MV | Register.MADCTL_RGB));
                    break;
                case RotationType._180Degrees:
                    SendData((byte)Register.MADCTL_RGB);
                    break;
                case RotationType._270Degrees:
                    SendData((byte)(Register.MADCTL_MX | Register.MADCTL_MV | Register.MADCTL_RGB));
                    break;
            }

            UpdateBuffer();
        }

        internal const byte HX8357_RDDID = 0x04;
        internal const byte HX8357_RDDST = 0x09;
        internal const byte HX8357_RDPOWMODE = 0x0A;
        internal const byte HX8357_RDMADCTL = 0x0B;
        internal const byte HX8357_RDCOLMOD = 0x0C;
        internal const byte HX8357_RDDIM = 0x0D;
        internal const byte HX8357_RDDSDR = 0x0F;
        internal const byte HX8357_TEON = 0x35;
        internal const byte HX8357_TEARLINE = 0x44;
        internal const byte HX8357_SETOSC = 0xB0;
        internal const byte HX8357_SETPWR1 = 0xB1;
        internal const byte HX8357_SETRGB = 0xB3;
        internal const byte HX8357D_SETCOM = 0xB6;
        internal const byte HX8357D_SETCYC = 0xB4;
        internal const byte HX8357D_SETC = 0xB9;
        internal const byte HX8357D_SETSTBA = 0xC0;
        internal const byte HX8357_SETPANEL = 0xCC;
        internal const byte HX8357D_SETGAMMA = 0xE0;
    }
}