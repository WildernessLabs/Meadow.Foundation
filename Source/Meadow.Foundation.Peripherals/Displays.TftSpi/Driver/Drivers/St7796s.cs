using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a St7796s TFT color display
    /// </summary>
    public class St7796s : TftSpiBase, IRotatableDisplay
    {
        /// <summary>
        /// The default display color mode
        /// </summary>
        public override ColorMode DefautColorMode => ColorMode.Format12bppRgb444;

        /// <summary>
        /// The color types supported by the display
        /// </summary>
        public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565 | ColorMode.Format12bppRgb444;

        /// <summary>
        /// Create a new St7796s color display object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public St7796s(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width = 320, int height = 480, ColorMode colorMode = ColorMode.Format12bppRgb444)
			: base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height, colorMode)
        {
            Initialize();

            SetRotation(RotationType.Normal);
        }

        /// <summary>
        /// Create a new St7796s color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public St7796s(ISpiBus spiBus, IDigitalOutputPort chipSelectPort,
                IDigitalOutputPort dataCommandPort, IDigitalOutputPort resetPort,
                int width = 320, int height = 480, ColorMode colorMode = ColorMode.Format12bppRgb444) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, width, height, colorMode)
        {
            Initialize();
        }

        /// <summary>
        /// Initalize the display
        /// </summary>
        protected override void Initialize()
        {
			DelayMs(120);

			SendCommand(Register.SWRESET); //Software reset
			DelayMs(120);

			SendCommand(Register.SLPOUT); //Sleep exit                                            
			DelayMs(120);

			SendCommand(0xF0); //Command Set control                                 
			SendData(0xC3);    //Enable extension command 2 partI

			SendCommand(0xF0); //Command Set control                                 
			SendData(0x96);    //Enable extension command 2 partII

			SendCommand(Register.MADCTL); //Memory Data Access Control MX, MY, RGB mode                                    
			SendData(0x48);    //X-Mirror, Top-Left to right-Buttom, RGB  

			SendCommand(Register.COLOR_MODE);  // set color mode
			if (ColorMode == ColorMode.Format16bppRgb565)
				SendData(0x05);  // 16-bit color RGB565
			else
				SendData(0x03); //12-bit color RGB444

			SendCommand(0xB4); //Column inversion 
			SendData(0x01);    //1-dot inversion

			SendCommand(0xB6); //Display Function Control
			SendData(0x80);    //Bypass
			SendData(0x02);    //Source Output Scan from S1 to S960, Gate Output scan from G1 to G480, scan cycle=2
			SendData(0x3B);    //LCD Drive Line=8*(59+1)

			SendCommand(0xE8); //Display Output Ctrl Adjust
			SendData(0x40);
			SendData(0x8A);
			SendData(0x00);
			SendData(0x00);
			SendData(0x29);    //Source eqaulizing period time= 22.5 us
			SendData(0x19);    //Timing for "Gate start"=25 (Tclk)
			SendData(0xA5);    //Timing for "Gate End"=37 (Tclk), Gate driver EQ function ON
			SendData(0x33);

			SendCommand(0xC1); //Power control2                          
			SendData(0x06);    //VAP(GVDD)=3.85+( vcom+vcom offset), VAN(GVCL)=-3.85+( vcom+vcom offset)

			SendCommand(0xC2); //Power control 3                                      
			SendData(0xA7);    //Source driving current level=low, Gamma driving current level=High

			SendCommand(0xC5); //VCOM Control
			SendData(0x18);    //VCOM=0.9

			DelayMs(120);

			//ST7796 Gamma Sequence
			SendCommand(0xE0); //Gamma"+"                                             
			SendData(0xF0);
			SendData(0x09);
			SendData(0x0b);
			SendData(0x06);
			SendData(0x04);
			SendData(0x15);
			SendData(0x2F);
			SendData(0x54);
			SendData(0x42);
			SendData(0x3C);
			SendData(0x17);
			SendData(0x14);
			SendData(0x18);
			SendData(0x1B);

			SendCommand(0xE1); //Gamma"-"                                             
			SendData(0xE0);
			SendData(0x09);
			SendData(0x0B);
			SendData(0x06);
			SendData(0x04);
			SendData(0x03);
			SendData(0x2B);
			SendData(0x43);
			SendData(0x42);
			SendData(0x3B);
			SendData(0x16);
			SendData(0x14);
			SendData(0x17);
			SendData(0x1B);

			DelayMs(120);

			SendCommand(0xF0); //Command Set control                                 
			SendData(0x3C);    //Disable extension command 2 partI

			SendCommand(0xF0); //Command Set control                                 
			SendData(0x69);    //Disable extension command 2 partII

			DelayMs(120);

			SendCommand(0x29); //Display on
		}

        /// <summary>
        /// Set addrees window for display updates
        /// </summary>
        /// <param name="x0">X start in pixels</param>
        /// <param name="y0">Y start in pixels</param>
        /// <param name="x1">X end in pixels</param>
        /// <param name="y1">Y end in pixels</param>
        protected override void SetAddressWindow(int x0, int y0, int x1, int y1)
		{
			SendCommand(LcdCommand.CASET);  // column addr set
			dataCommandPort.State = Data;
			Write((byte)(x0 >> 8));
			Write((byte)(x0 & 0xff));   // XSTART 
			Write((byte)(x1 >> 8));
			Write((byte)(x1 & 0xff));   // XEND

			SendCommand(LcdCommand.RASET);  // row addr set
			dataCommandPort.State = Data;
			Write((byte)(y0 >> 8));
			Write((byte)(y0 & 0xff));    // YSTART
			Write((byte)(y1 >> 8));
			Write((byte)(y1 & 0xff));    // YEND

			SendCommand(LcdCommand.RAMWR);  // write to RAM
		}

        /// <summary>
        /// Set the display rotation
        /// </summary>
        /// <param name="rotation">The rotation value</param>
        public void SetRotation(RotationType rotation)
        {
            SendCommand((byte)Register.MADCTL);

            switch (rotation)
            {
                case RotationType.Normal:
                    SendData((byte)Register.MADCTL_MX | (byte)Register.MADCTL_BGR);
                    break;
                case RotationType._90Degrees:
                    SendData((byte)Register.MADCTL_MV | (byte)Register.MADCTL_BGR);
                    break;
                case RotationType._180Degrees:
                    SendData((byte)Register.MADCTL_BGR | (byte)Register.MADCTL_MY);
                    break;
                case RotationType._270Degrees:
                    SendData((byte)Register.MADCTL_BGR | (byte)Register.MADCTL_MV | (byte)Register.MADCTL_MX | (byte)Register.MADCTL_MY);
                    break;
            }
        }
    }
}