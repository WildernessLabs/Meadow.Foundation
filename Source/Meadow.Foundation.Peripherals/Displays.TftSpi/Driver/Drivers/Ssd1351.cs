using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using Meadow.Units;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a Ssd1351 TFT color display
    /// </summary>
    public class Ssd1351 : TftSpiBase
    {
        /// <summary>
        /// The default SPI bus frequency
        /// </summary>
        public static Frequency DefaultSpiBusSpeed = new Frequency(12000, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// The default display color mode
        /// </summary>
        public override ColorType DefautColorMode => ColorType.Format16bppRgb565;

        /// <summary>
        /// Create a new Ssd1351 color display object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Ssd1351(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width, int height)
            : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height, ColorType.Format16bppRgb565)
        {
            Initialize();
        }

        /// <summary>
        /// Create a new Ssd1351 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Ssd1351(ISpiBus spiBus, IDigitalOutputPort chipSelectPort,
                IDigitalOutputPort dataCommandPort, IDigitalOutputPort resetPort,
                int width, int height) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, width, height, ColorType.Format16bppRgb565)
        {
            Initialize();
        }

        protected override void Initialize()
        {
            // Initialization Sequence
            if (resetPort != null)
            {
                resetPort.State = true;
                Thread.Sleep(50);
                resetPort.State = false;
                Thread.Sleep(50);
                resetPort.State = true;
                Thread.Sleep(50);
            }
            else
            {
                Thread.Sleep(150); //Not sure if this is needed but can't hurt
            }

            SendCommand(CMD_COMMANDLOCK);
            SendData(0x12);

            SendCommand(CMD_COMMANDLOCK);
            SendData(0xB1);

            SendCommand(CMD_DISPLAYOFF);
            SendCommand(CMD_CLOCKDIV);
            SendData(0xF1);

            SendCommand(CMD_MUXRATIO);
            SendData(0x7F);

            SendCommand(CMD_SETCOLUMN); //column address
            SendData(new byte[] { 0x00, (byte)(Width - 1) });

            SendCommand(CMD_SETROW); //row address
            SendData(new byte[] { 0x00, (byte)(Height - 1) });

            SendCommand(CMD_SETREMAP);
            SendData(new byte[] { 0x70, 0x04 }); //change 2nd value to 0x04 for BGR

            SendCommand(CMD_DISPLAYOFFSET);
            if (Height == 96)
                SendData(96);
            else
                SendData(0x0);

            SendCommand(CMD_SETGPIO);
            SendData(0x0);

            SendCommand(CMD_FUNCTIONSELECT);
            SendData(0x01);

            SendCommand(CMD_PRECHARGE);
            SendData(0x32);

            SendCommand(CMD_VCOMH);
            SendData(0x05);

            SendCommand(CMD_NORMALDISPLAY);

            SendCommand(CMD_CONTRASTABC);
            SendData(new byte[] { 0xC8, 0x80, 0xC8 });

            SendCommand(CMD_CONTRASTMASTER);
            SendData(0x0F);

            SendCommand(CMD_SETVSL);
            SendData(new byte[] { 0xA0, 0xB5, 0x55 });

            SendCommand(CMD_PRECHARGE2);
            SendData(0x01);

            SendCommand(CMD_DISPLAYON);
            
            SetAddressWindow(0, 0, (Width - 1), (Height - 1));

            dataCommandPort.State = Data;
        }

        public override bool IsColorModeSupported(ColorType mode)
        {
            if (mode == ColorType.Format16bppRgb565)
            {
                return true;
            }
            return false;
        }

        //looks like this display only supports dimensions of 255 or less
        protected override void SetAddressWindow(int x0, int y0, int x1, int y1)
        {
            SendCommand(CMD_SETCOLUMN);  // column addr set
            SendData((byte)x0);
            SendData((byte)x1);

            SendCommand(CMD_SETROW);  // row addr set
            SendData((byte)y0);
            SendData((byte)y1);

            SendCommand(CMD_WRITERAM);
        }

        private void InvertDisplay (bool invert)
        {
            SendCommand(invert ? CMD_INVERTDISPLAY : CMD_NORMALDISPLAY);
        }

        static byte CMD_SETCOLUMN      = 0x15;
        static byte CMD_SETROW         = 0x75;
        static byte CMD_WRITERAM       = 0x5C;
        //static byte CMD_READRAM        = 0x5D;
        static byte CMD_SETREMAP       = 0xA0;
        //static byte CMD_STARTLINE      = 0xA1;
        static byte CMD_DISPLAYOFFSET  = 0xA2;
        //static byte CMD_DISPLAYALLOFF  = 0xA4;
        //static byte CMD_DISPLAYALLON   = 0xA5;
        static byte CMD_NORMALDISPLAY  = 0xA6;
        static byte CMD_INVERTDISPLAY  = 0xA7;
        static byte CMD_FUNCTIONSELECT = 0xAB;
        static byte CMD_DISPLAYOFF     = 0xAE;
        static byte CMD_DISPLAYON      = 0xAF;
        static byte CMD_PRECHARGE      = 0xB1;
        //static byte CMD_DISPLAYENHANCE = 0xB2;
        static byte CMD_CLOCKDIV       = 0xB3;
        static byte CMD_SETVSL         = 0xB4;
        static byte CMD_SETGPIO        = 0xB5;
        static byte CMD_PRECHARGE2     = 0xB6;
        //static byte CMD_SETGRAY        = 0xB8;
        //static byte CMD_USELUT         = 0xB9;
        //static byte CMD_PRECHARGELEVEL = 0xBB;
        static byte CMD_VCOMH          = 0xBE;
        static byte CMD_CONTRASTABC    = 0xC1;
        static byte CMD_CONTRASTMASTER = 0xC7;
        static byte CMD_MUXRATIO       = 0xCA;
        static byte CMD_COMMANDLOCK    = 0xFD;
        //static byte CMD_HORIZSCROLL    = 0x96;
        //static byte CMD_STOPSCROLL     = 0x9E;
        //static byte CMD_STARTSCROLL    = 0x9F;

    }
}
