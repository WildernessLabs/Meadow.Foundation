using Meadow.Devices;
using Meadow.Hardware;
using System.Threading;

namespace Meadow.Foundation.Displays.TftSpi
{
    public class Hx8357b : Hx8357d
    {
        public Hx8357b(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width = 320, int height = 480, DisplayColorMode displayColorMode = DisplayColorMode.Format16bppRgb565)
            : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height, displayColorMode)
        {
            Initialize();

            SetRotation(Rotation.Normal);
        }

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
            SendCommand(RegisterHX8357B.SETDISPMODE);
            SendData(0x00); // CPU (DBI) and internal oscillation ??
            SendCommand(HX8357_SLPOUT);
            Thread.Sleep(120);
            SendCommand(HX8357_DISPON);
            Thread.Sleep(10);
        }

        protected void SendCommand(RegisterHX8357B command)
        {
            SendCommand((byte)command);
        }

        protected enum RegisterHX8357B : byte
        {
            PTLON = 0x12, ///< Partial mode on
            NORON = 0x13, ///< Normal mode
            PTLAR = 0x30,   ///< (unknown)
            SETDISPLAY = 0xB2, ///< Set display mode
            SETDISPMODE = 0xB4, ///< Set display mode
            SETOTP = 0xB7,      ///< Set OTP memory
            SET_PANEL_DRIVING = 0xC0, ///< Set panel drive mode
            SETDGC = 0xC1,            ///< Set DGC settings
            SETID = 0xC3,             ///< Set ID
            SETDDB = 0xC4,            ///< Set DDB
            SETDISPLAYFRAME = 0xC5,   ///< Set display frame
            GAMMASET = 0xC8,          ///< Set Gamma correction
            SETCABC = 0xC9,           ///< Set CABC
            SETPOWER = 0xD0,     ///< Set power control
            SETVCOM = 0xD1,      ///< Set VCOM
            SETPWRNORMAL = 0xD2, ///< Set power normal
            RDID1 = 0xDA, ///< Read ID #1
            RDID2 = 0xDB, ///< Read ID #2
            RDID3 = 0xDC, ///< Read ID #3
            RDID4 = 0xDD, ///< Read ID #4
            SETGAMMA = 0xC8,        ///< Set Gamma
            SETPANELRELATED = 0xE9, ///< Set panel related
        }
    }
}