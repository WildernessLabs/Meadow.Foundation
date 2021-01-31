using Meadow.Hardware;
using System.Threading;

namespace Meadow.Foundation.Displays.Tft
{
    public class Ssd1351 : TftSpiBase
    {
        public override DisplayColorMode DefautColorMode => DisplayColorMode.Format16bppRgb565;

        public Ssd1351(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width, int height)
            : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height, DisplayColorMode.Format16bppRgb565)
        {
            Initialize();
        }

        protected override void Initialize()
        {
            // Initialization Sequence
            resetPort.State = (true);
            Thread.Sleep(50);
            resetPort.State = (false);
            Thread.Sleep(50);
            resetPort.State = (true);

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
            SendData(new byte[] { 0x00, (byte)(width - 1) });

            SendCommand(CMD_SETROW); //row address
            SendData(new byte[] { 0x00, (byte)(height - 1) });

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
            
            SetAddressWindow(0, 0, (width - 1), (height - 1));

            dataCommandPort.State = Data;
        }

        public override bool IsColorModeSupported(DisplayColorMode mode)
        {
            if (mode == DisplayColorMode.Format16bppRgb565)
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

        static byte CMD_SETCOLUMN      = 0x15; ///< See datasheet
        static byte CMD_SETROW         = 0x75; ///< See datasheet
        static byte CMD_WRITERAM       = 0x5C; ///< See datasheet
        //static byte CMD_READRAM        = 0x5D; ///< Not currently used
        static byte CMD_SETREMAP       = 0xA0; ///< See datasheet
        //static byte CMD_STARTLINE      = 0xA1; ///< See datasheet
        static byte CMD_DISPLAYOFFSET  = 0xA2; ///< See datasheet
        //static byte CMD_DISPLAYALLOFF  = 0xA4; ///< Not currently used
        //static byte CMD_DISPLAYALLON   = 0xA5; ///< Not currently used
        static byte CMD_NORMALDISPLAY  = 0xA6; ///< See datasheet
        static byte CMD_INVERTDISPLAY  = 0xA7; ///< See datasheet
        static byte CMD_FUNCTIONSELECT = 0xAB; ///< See datasheet
        static byte CMD_DISPLAYOFF     = 0xAE; ///< See datasheet
        static byte CMD_DISPLAYON      = 0xAF; ///< See datasheet
        static byte CMD_PRECHARGE      = 0xB1; ///< See datasheet
        //static byte CMD_DISPLAYENHANCE = 0xB2; ///< Not currently used
        static byte CMD_CLOCKDIV       = 0xB3; ///< See datasheet
        static byte CMD_SETVSL         = 0xB4; ///< See datasheet
        static byte CMD_SETGPIO        = 0xB5; ///< See datasheet
        static byte CMD_PRECHARGE2     = 0xB6; ///< See datasheet
        //static byte CMD_SETGRAY        = 0xB8; ///< Not currently used
        //static byte CMD_USELUT         = 0xB9; ///< Not currently used
        //static byte CMD_PRECHARGELEVEL = 0xBB; ///< Not currently used
        static byte CMD_VCOMH          = 0xBE; ///< See datasheet
        static byte CMD_CONTRASTABC    = 0xC1; ///< See datasheet
        static byte CMD_CONTRASTMASTER = 0xC7; ///< See datasheet
        static byte CMD_MUXRATIO       = 0xCA; ///< See datasheet
        static byte CMD_COMMANDLOCK    = 0xFD; ///< See datasheet
        //static byte CMD_HORIZSCROLL    = 0x96; ///< Not currently used
        //static byte CMD_STOPSCROLL     = 0x9E; ///< Not currently used
        //static byte CMD_STARTSCROLL    = 0x9F; ///< Not currently used

    }
}
