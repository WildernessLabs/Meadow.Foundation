using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    public class SSD1331 : DisplayTFTSpiBase
    {
        public override DisplayColorMode ColorMode => DisplayColorMode.Format16bppRgb565;


        public override uint Width => _width;

        public override uint Height => _height;

        private SSD1331()
        {

        }

        public SSD1331(IDigitalPin chipSelectPin, IDigitalPin dcPin, IDigitalPin resetPin,
            uint width, uint height,
            Spi.SPI_module spiModule = Spi.SPI_module.SPI1,
            uint speedKHz = 9500) : base(chipSelectPin, dcPin, resetPin, width, height, spiModule, speedKHz)
        {
            _width = width;
            _height = height;
            Initialize();
        }

        protected override void Initialize()
        {
            // Initialization Sequence

            SendCommand(SSD1331_CMD_DISPLAYOFF);   // 0xAE
            SendCommand(SSD1331_CMD_SETREMAP);     // 0xA0
            SendData(0x72);				// RGB Color
            //SendCommand(0x76);             // BGR Color
            SendCommand(SSD1331_CMD_STARTLINE);    // 0xA1
            SendData(0x0);
            SendCommand(SSD1331_CMD_DISPLAYOFFSET);    // 0xA2
            SendData(0x0);
            SendCommand(SSD1331_CMD_NORMALDISPLAY);    // 0xA4
            SendCommand(SSD1331_CMD_SETMULTIPLEX);     // 0xA8
            SendData(0x3F);             // 0x3F 1/64 duty
            SendCommand(SSD1331_CMD_SETMASTER);    // 0xAD
            SendData(0x8E);
            SendCommand(SSD1331_CMD_POWERMODE);    // 0xB0
            SendData(0x0B);
            SendCommand(SSD1331_CMD_PRECHARGE);    // 0xB1
            SendData(0x31);
            SendCommand(SSD1331_CMD_CLOCKDIV);     // 0xB3
            SendData(0xF0);  // 7:4 = Oscillator Frequency, 3:0 = CLK Div Ratio (A[3:0]+1 = 1..16)
            SendCommand(SSD1331_CMD_PRECHARGEA);   // 0x8A
            SendData(0x64);

            SendCommand(SSD1331_CMD_PRECHARGEB);   // 0x8B
            SendData(0x78);

            SendCommand(SSD1331_CMD_PRECHARGEA);   // 0x8C
            SendData(0x64);

            SendCommand(SSD1331_CMD_PRECHARGELEVEL);   // 0xBB
            SendData(0x3A);

            SendCommand(SSD1331_CMD_VCOMH);        // 0xBE
            SendData(0x3E);

            SendCommand(SSD1331_CMD_MASTERCURRENT);    // 0x87
            SendData(0x06);

            SendCommand(SSD1331_CMD_CONTRASTA);    // 0x81
            SendData(0x91);

            SendCommand(SSD1331_CMD_CONTRASTB);    // 0x82
            SendData(0x50);

            SendCommand(SSD1331_CMD_CONTRASTC);    // 0x83
            SendData(0x7D);

            SendCommand(SSD1331_CMD_DISPLAYON);	//--turn on oled panel   

            SetAddressWindow(0, 0, (byte)(_width - 1), (byte)(_height - 1));
        }

        private void SetAddressWindow(byte x0, byte y0, byte x1, byte y1)
        {
            SendCommand(0x15);  // column addr set
            SendData(0x00);
            SendData(x0);   // XSTART 
            SendData(0x00);
            SendData(x1);   // XEND

            SendCommand(0x75);  // row addr set
            SendData(0x00);
            SendData(y0);    // YSTART
            SendData(0x00);
            SendData(y1);    // YEND
        }
        
        // Timing Delays
        static int SSD1331_DELAYS_HWFILL = 3;
        static int SSD1331_DELAYS_HWLINE = 1;

        // SSD1331 Commands
        static byte SSD1331_CMD_DRAWLINE        = 0x21;
        static byte SSD1331_CMD_DRAWRECT 		= 0x22;
        static byte SSD1331_CMD_FILL 			= 0x26;
        static byte SSD1331_CMD_SETCOLUMN 		= 0x15;
        static byte SSD1331_CMD_SETROW    		= 0x75;
        static byte SSD1331_CMD_CONTRASTA 		= 0x81;
        static byte SSD1331_CMD_CONTRASTB 		= 0x82;
        static byte SSD1331_CMD_CONTRASTC		= 0x83;
        static byte SSD1331_CMD_MASTERCURRENT 	= 0x87;
        static byte SSD1331_CMD_SETREMAP 		= 0xA0;
        static byte SSD1331_CMD_STARTLINE 		= 0xA1;
        static byte SSD1331_CMD_DISPLAYOFFSET 	= 0xA2;
        static byte SSD1331_CMD_NORMALDISPLAY 	= 0xA4;
        static byte SSD1331_CMD_DISPLAYALLON  	= 0xA5;
        static byte SSD1331_CMD_DISPLAYALLOFF 	= 0xA6;
        static byte SSD1331_CMD_INVERTDISPLAY 	= 0xA7;
        static byte SSD1331_CMD_SETMULTIPLEX  	= 0xA8;
        static byte SSD1331_CMD_SETMASTER 		= 0xAD;
        static byte SSD1331_CMD_DISPLAYOFF 		= 0xAE;
        static byte SSD1331_CMD_DISPLAYON     	= 0xAF;
        static byte SSD1331_CMD_POWERMODE 		= 0xB0;
        static byte SSD1331_CMD_PRECHARGE 		= 0xB1;
        static byte SSD1331_CMD_CLOCKDIV 		= 0xB3;
        static byte SSD1331_CMD_PRECHARGEA 		= 0x8A;
        static byte SSD1331_CMD_PRECHARGEB 		= 0x8B;
        static byte SSD1331_CMD_PRECHARGEC 		= 0x8C;
        static byte SSD1331_CMD_PRECHARGELEVEL 	= 0xBB;
        static byte SSD1331_CMD_VCOMH 			= 0xBE;
    }
}