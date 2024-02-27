using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a Ssd1331 S6D02A1 TFT color display
    /// </summary>
    public class Ssd1331 : TftSpiBase
    {
        //the SSD1331 also supports 8 bit RGB332 color but this isn't currently supported (but should be quick to add if anyone wants it
        /// <summary>
        /// The default display color mode
        /// </summary>
        public override ColorMode DefaultColorMode => ColorMode.Format16bppRgb565;

        /// <summary>
        /// The color modes supported by the display
        /// </summary>
        public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565;

        /// <summary>
        /// Create a new Ssd1331 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Ssd1331(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
           int width = 96, int height = 64)
            : base(spiBus, chipSelectPin, dcPin, resetPin, width, height, ColorMode.Format16bppRgb565)
        {
            Initialize();
        }

        /// <summary>
        /// Create a new Ssd1331 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Ssd1331(ISpiBus spiBus, IDigitalOutputPort chipSelectPort,
                IDigitalOutputPort dataCommandPort, IDigitalOutputPort resetPort,
                int width = 96, int height = 64) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, width, height, ColorMode.Format16bppRgb565)
        {
            Initialize();
        }

        /// <summary>
        /// Initialize the display
        /// </summary>
        protected override void Initialize()
        {
            // Initialization Sequence
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

            SendCommand(CMD_DISPLAYOFF);   // 0xAE
            SendCommand(CMD_SETREMAP);     // 0xA0
            SendCommand(0x72);				// RGB Color
            //SendCommand(0x76);             // BGR Color
            SendCommand(CMD_STARTLINE);    // 0xA1
            SendCommand((byte)0x0);
            SendCommand(CMD_DISPLAYOFFSET);    // 0xA2
            SendCommand((byte)0x0);
            SendCommand(CMD_NORMALDISPLAY);    // 0xA4
            SendCommand(CMD_SETMULTIPLEX);     // 0xA8
            SendCommand(0x3F);             // 0x3F 1/64 duty
            SendCommand(CMD_SETMASTER);    // 0xAD
            SendCommand(0x8E);
            SendCommand(CMD_POWERMODE);    // 0xB0
            SendCommand(0x0B);
            SendCommand(CMD_PRECHARGE);    // 0xB1
            SendCommand(0x31);
            SendCommand(CMD_CLOCKDIV);     // 0xB3
            SendCommand(0xF0);  // 7:4 = Oscillator Frequency, 3:0 = CLK Div Ratio (A[3:0]+1 = 1..16)
            SendCommand(CMD_PRECHARGEA);   // 0x8A
            SendCommand(0x64);

            SendCommand(CMD_PRECHARGEB);   // 0x8B
            SendCommand(0x78);

            SendCommand(CMD_PRECHARGEA);   // 0x8C
            SendCommand(0x64);

            SendCommand(CMD_PRECHARGELEVEL);   // 0xBB
            SendCommand(0x3A);

            SendCommand(CMD_VCOMH);        // 0xBE
            SendCommand(0x3E);

            SendCommand(CMD_MASTERCURRENT);    // 0x87
            SendCommand(0x06);

            SendCommand(CMD_CONTRASTA);    // 0x81
            SendCommand(0x91);

            SendCommand(CMD_CONTRASTB);    // 0x82
            SendCommand(0x50);

            SendCommand(CMD_CONTRASTC);    // 0x83
            SendCommand(0x7D);

            SendCommand(CMD_DISPLAYON);	//--turn on oled panel   

            dataCommandPort.State = Data;
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
            SendCommand(0x15);  // column addr set
            SendCommand((byte)x0);
            SendCommand((byte)x1);

            SendCommand(0x75);  // row addr set
            SendCommand((byte)y0);
            SendCommand((byte)y1);
        }

        // SSD1331 Commands
        static readonly byte CMD_CONTRASTA = 0x81;
        static readonly byte CMD_CONTRASTB = 0x82;
        static readonly byte CMD_CONTRASTC = 0x83;
        static readonly byte CMD_MASTERCURRENT = 0x87;
        static readonly byte CMD_SETREMAP = 0xA0;
        static readonly byte CMD_STARTLINE = 0xA1;
        static readonly byte CMD_DISPLAYOFFSET = 0xA2;
        static readonly byte CMD_NORMALDISPLAY = 0xA4;
        static readonly byte CMD_SETMULTIPLEX = 0xA8;
        static readonly byte CMD_SETMASTER = 0xAD;
        static readonly byte CMD_DISPLAYOFF = 0xAE;
        static readonly byte CMD_DISPLAYON = 0xAF;
        static readonly byte CMD_POWERMODE = 0xB0;
        static readonly byte CMD_PRECHARGE = 0xB1;
        static readonly byte CMD_CLOCKDIV = 0xB3;
        static readonly byte CMD_PRECHARGEA = 0x8A;
        static readonly byte CMD_PRECHARGEB = 0x8B;
        static readonly byte CMD_PRECHARGELEVEL = 0xBB;
        static readonly byte CMD_VCOMH = 0xBE;
    }
}