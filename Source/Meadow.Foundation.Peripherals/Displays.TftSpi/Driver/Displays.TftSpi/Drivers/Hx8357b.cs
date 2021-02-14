using Meadow.Hardware;
using System.Threading;

namespace Meadow.Foundation.Displays.Tft
{
    public class Hx8357b : Hx8357d
    {
        public Hx8357b(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width = 320, int height = 480, DisplayColorMode displayColorMode = DisplayColorMode.Format16bppRgb565)
            : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height, displayColorMode)
        {
            Initialize();

            SetRotation(Rotation.Normal);
        }

        protected override void Initialize()
        {
            SendCommand(HX8357B_SETPOWER);
            SendData(0x44);
            SendData(0x41);
            SendData(0x06);
            SendCommand(HX8357B_SETVCOM);
            SendData(0x40);
            SendData(0x10);
            SendCommand(HX8357B_SETPWRNORMAL);
            SendData(0x05);
            SendData(0x12);
            SendCommand(HX8357B_SET_PANEL_DRIVING);
            SendData(0x14);
            SendData(0x3b);
            SendData(0x00);
            SendData(0x02);
            SendData(0x11);
            SendCommand(HX8357B_SETDISPLAYFRAME);
            SendData(0x0c); // 6.8mhz
            SendCommand(HX8357B_SETPANELRELATED);
            SendData(0x01); // BGR
            // SendData(0xEA);
            // seq_undefined1, 3 args
            SendData(0x03);
            SendData(0x00);
            SendData(0x00);
            // SendData(0xEB);
            // undef2, 4 args
            SendData(0x40);
            SendData(0x54);
            SendData(0x26);
            SendData(0xdb);
            SendCommand(HX8357B_SETGAMMA);
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
            SendCommand(MADCTL);
            SendData(0xC0);
            SendCommand(COLOR_MODE);
            SendData(0x55);
            SendCommand((byte)LcdCommand.RASET);
            SendData(0x00);
            SendData(0x00);
            SendData(0x01);
            SendData(0xDF);
            SendCommand((byte)LcdCommand.CASET);
            SendData(0x00);
            SendData(0x00);
            SendData(0x01);
            SendData(0x3F);
            SendCommand(HX8357B_SETDISPMODE);
            SendData(0x00); // CPU (DBI) and internal oscillation ??
            SendCommand(HX8357_SLPOUT);
            Thread.Sleep(120);
            SendCommand(HX8357_DISPON);
            Thread.Sleep(10);
        }

        const byte HX8357B_PTLON = 0x12; ///< Partial mode on
        const byte HX8357B_NORON = 0x13; ///< Normal mode
        const byte HX8357B_PTLAR = 0x30;   ///< (unknown)
        const byte HX8357B_SETDISPLAY = 0xB2; ///< Set display mode
        const byte HX8357B_SETDISPMODE = 0xB4; ///< Set display mode
        const byte HX8357B_SETOTP = 0xB7;      ///< Set OTP memory
        const byte HX8357B_SET_PANEL_DRIVING = 0xC0; ///< Set panel drive mode
        const byte HX8357B_SETDGC = 0xC1;            ///< Set DGC settings
        const byte HX8357B_SETID = 0xC3;             ///< Set ID
        const byte HX8357B_SETDDB = 0xC4;            ///< Set DDB
        const byte HX8357B_SETDISPLAYFRAME = 0xC5;   ///< Set display frame
        const byte HX8357B_GAMMASET = 0xC8;          ///< Set Gamma correction
        const byte HX8357B_SETCABC = 0xC9;           ///< Set CABC
        const byte HX8357B_SETPOWER = 0xD0;     ///< Set power control
        const byte HX8357B_SETVCOM = 0xD1;      ///< Set VCOM
        const byte HX8357B_SETPWRNORMAL = 0xD2; ///< Set power normal
        const byte HX8357B_RDID1 = 0xDA; ///< Read ID #1
        const byte HX8357B_RDID2 = 0xDB; ///< Read ID #2
        const byte HX8357B_RDID3 = 0xDC; ///< Read ID #3
        const byte HX8357B_RDID4 = 0xDD; ///< Read ID #4
        const byte HX8357B_SETGAMMA = 0xC8;        ///< Set Gamma
        const byte HX8357B_SETPANELRELATED = 0xE9; ///< Set panel related
    }
}