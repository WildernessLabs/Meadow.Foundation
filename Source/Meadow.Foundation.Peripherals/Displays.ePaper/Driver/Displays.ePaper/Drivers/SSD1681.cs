using Meadow.Foundation.Displays.ePaper;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.ePaper
{

    /// <summary>
    ///     Represents an Ssd1681 ePaper B/W or color display commonly 1.54"
    ///     200x200, e-Ink three-color display, SPI interface 
    ///     DRIVER NOT COMPLETE
    /// </summary>
    public class Ssd1681 : EpdColorBase
    {
        protected override bool IsBlackInverted => false;

        protected override bool IsColorInverted => false;

        static byte DRIVER_CONTROL = 0x01;
        static byte GATE_VOLTAGE = 0x03;
        static byte SOURCE_VOLTAGE = 0x04;
        static byte INIT_SETTING = 0x08;
        static byte INIT_WRITE_REG = 0x09;
        static byte INIT_READ_REG = 0x0A;
        new static byte BOOSTER_SOFT_START = 0x0C;
        new static byte DEEP_SLEEP = 0x10;
        static byte DATA_MODE = 0x11;
        static byte SW_RESET = 0x12;
        static byte HV_DETECT = 0x14;
        static byte VCI_DETECT = 0x15;
        static byte TEMP_CONTROL = 0x18;
        static byte TEMP_WRITE = 0x1A;
        static byte TEMP_READ = 0x1B;
        static byte EXTTEMP_WRITE = 0x1C;
        static byte MASTER_ACTIVATE = 0x20;
        static byte DISP_CTRL1 = 0x21;
        static byte DISP_CTRL2 = 0x22;
        static byte WRITE_BWRAM = 0x24;
        static byte WRITE_REDRAM = 0x26;
        static byte READ_RAM = 0x27;
        static byte VCOM_SENSE = 0x28;
        static byte VCOM_DURATION = 0x29;
        static byte WRITE_VCOM_OTP = 0x2A;
        static byte WRITE_VCOM_CTRL = 0x2B;
        static byte WRITE_VCOM_REG = 0x2C;
        static byte READ_OTP = 0x2D;
        static byte READ_USERID = 0x2E;
        static byte READ_STATUS = 0x2F;
        static byte WRITE_WS_OTP = 0x30;
        static byte LOAD_WS_OTP = 0x31;
        static byte WRITE_LUT = 0x32;
        static byte CRC_CALC = 0x34;
        static byte CRC_READ = 0x35;
        static byte PROG_OTP = 0x36;
        static byte WRITE_DISPLAY_OPT = 0x37;
        static byte WRITE_USERID = 0x38;
        static byte OTP_PROGMODE = 0x39;
        static byte WRITE_BORDER = 0x3C;
        static byte END_OPTION = 0x3F;
        static byte SET_RAMXPOS = 0x44;
        static byte SET_RAMYPOS = 0x45;
        static byte AUTOWRITE_RED = 0x46;
        static byte AUTOWRITE_BW = 0x47;
        static byte SET_RAMXCOUNT = 0x4E;
        static byte SET_RAMYCOUNT = 0x4F;
        static byte NOP = 0xFF;

        public static byte[] LutData = { 0x02, 0x02, 0x01, 0x11, 0x12, 0x12 }; //""fiiYX\x99\x99\x88\x00\x00\x00\x00\xf8\xb4\x13Q5QQ\x19\x01\x00' };
        //_LUT_DATA = b'\x02\x02\x01\x11\x12\x12""fiiYX\x99\x99\x88\x00\x00\x00\x00\xf8\xb4\x13Q5QQ\x19\x01\x00'

        public Ssd1681(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width, int height) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
        {
        }

        protected override void Initialize()
        {
            Reset();

            SendCommand(SW_RESET);
            SendCommand(DRIVER_CONTROL);
            SendData(new byte[] { (byte)(Width - 1), (byte)((Height - 1) >> 8), 0x0 });

            SendCommand(DATA_MODE);
            SendData(0x03);

            SendCommand(SET_RAMXPOS);
            SendData(new byte[] { 0x0, (byte)(Height / 8 - 1) });

            SendCommand(SET_RAMYPOS);
            SendData(new byte[] { 0x0, 0x0, (byte)(Height - 1), (byte)((Height - 1) >> 8) });

            SendCommand(WRITE_BORDER);
            SendData(0x05);

            SendCommand(TEMP_CONTROL);
            SendData(0x80);

            WaitUntilIdle();
        }

        protected override void Refresh()
        {
            SendCommand(WRITE_BWRAM);

        
        }

        public void PowerDown()
        {
            SendCommand(DEEP_SLEEP);
            SendData(0x01);
        }

        public void Update()
        {
            SendCommand(DISP_CTRL2);
            SendData(0x07);
            SendCommand(MASTER_ACTIVATE);
            WaitUntilIdle();

        }
    }
}
