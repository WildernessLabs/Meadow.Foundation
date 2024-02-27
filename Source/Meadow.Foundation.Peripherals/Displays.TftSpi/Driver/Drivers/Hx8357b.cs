using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a Hx8357b TFT color display
    /// </summary>
    public class Hx8357b : Hx8357d
    {
        /// <summary>
        /// Create a new Hx8357b color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public Hx8357b(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width = 320, int height = 480, ColorMode colorMode = ColorMode.Format16bppRgb565)
            : base(spiBus, chipSelectPin, dcPin, resetPin, width, height, colorMode)
        {
            Initialize();

            SetRotation(RotationType.Normal);
        }

        /// <summary>
        /// Create a new Hx8357b color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public Hx8357b(ISpiBus spiBus, IDigitalOutputPort chipSelectPort,
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
            SendCommand(RegisterHX8357B.SETPOWER);
            SendData(0x44);
            SendData(0x41);
            SendData(0x06);
            SendCommand(RegisterHX8357B.SETVCOM);
            SendData(0x40);
            SendData(0x10);
            SendCommand(RegisterHX8357B.SETPWRNORMAL);
            SendData(0x05);
            SendData(0x12);
            SendCommand(RegisterHX8357B.SET_PANEL_DRIVING);
            SendData(0x14);
            SendData(0x3b);
            SendData(0x00);
            SendData(0x02);
            SendData(0x11);
            SendCommand(RegisterHX8357B.SETDISPLAYFRAME);
            SendData(0x0c); // 6.8mhz
            SendCommand(RegisterHX8357B.SETPANELRELATED);
            SendData(0x01); // BGR

            SendData(0x03);
            SendData(0x00);
            SendData(0x00);

            SendData(0x40);
            SendData(0x54);
            SendData(0x26);
            SendData(0xdb);
            SendCommand(RegisterHX8357B.SETGAMMA);
            SendData(0x00);
            SendData(0x15);
            SendData(0x00);
            SendData(0x22);
            SendData(0x00);
            SendData(0x08);
            SendData(0x77);
            SendData(0x26);
            SendData(0x66);
            SendData(0x22);
            SendData(0x04);
            SendData(0x00);
            SendCommand(Register.MADCTL);
            SendData(0xC0);
            SendCommand(Register.COLOR_MODE);
            SendData(0x55);
            SendCommand(LcdCommand.RASET);
            SendData(0x00);
            SendData(0x00);
            SendData(0x01);
            SendData(0xDF);
            SendCommand(LcdCommand.CASET);
            SendData(0x00);
            SendData(0x00);
            SendData(0x01);
            SendData(0x3F);
            SendCommand(RegisterHX8357B.SETDISPMODE);
            SendData(0x00); // CPU (DBI) and internal oscillation ??
            SendCommand(Register.SLPOUT);
            DelayMs(120);
            SendCommand(Register.DISPON);
            DelayMs(10);
        }

        /// <summary>
        /// Send a command to the display
        /// </summary>
        /// <param name="command">The command to send</param>
        protected void SendCommand(RegisterHX8357B command)
        {
            SendCommand((byte)command);
        }

        /// <summary>
        /// Display registers
        /// </summary>
        protected enum RegisterHX8357B : byte
        {
            /// <summary>
            /// Unknown
            /// </summary>
            PTLAR = 0x30,
            /// <summary>
            /// Set display
            /// </summary>
            SETDISPLAY = 0xB2,
            /// <summary>
            /// Set display mode
            /// </summary>
            SETDISPMODE = 0xB4,
            /// <summary>
            /// Set OTP memory
            /// </summary>
            SETOTP = 0xB7,
            /// <summary>
            /// Set panel drive mode
            /// </summary>
            SET_PANEL_DRIVING = 0xC0,
            /// <summary>
            /// Set DGC settings
            /// </summary>
            SETDGC = 0xC1,
            /// <summary>
            /// Set ID
            /// </summary>
            SETID = 0xC3,
            /// <summary>
            /// Set DDB
            /// </summary>
            SETDDB = 0xC4,
            /// <summary>
            /// Set display frame
            /// </summary>
            SETDISPLAYFRAME = 0xC5,
            /// <summary>
            /// Set gamma correction
            /// </summary>
            GAMMASET = 0xC8,
            /// <summary>
            /// Set CABC
            /// </summary>
            SETCABC = 0xC9,
            /// <summary>
            /// Set power control
            /// </summary>
            SETPOWER = 0xD0,
            /// <summary>
            /// Set VCOM
            /// </summary>
            SETVCOM = 0xD1,
            /// <summary>
            /// Set power normal
            /// </summary>
            SETPWRNORMAL = 0xD2,
            /// <summary>
            /// Read ID #1
            /// </summary>
            RDID1 = 0xDA,
            /// <summary>
            /// Read ID #2
            /// </summary>
            RDID2 = 0xDB,
            /// <summary>
            /// Read ID #3
            /// </summary>
            RDID3 = 0xDC,
            /// <summary>
            /// Set Gamma
            /// </summary>
            SETGAMMA = 0xC8,
            /// <summary>
            /// Set panel related
            /// </summary>
            SETPANELRELATED = 0xE9,
        }
    }
}