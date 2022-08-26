using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{

    /// <summary>
    /// Represents an Ssd1681 ePaper B/W or color display commonly 1.54"
    /// 200x200, e-Ink three-color display, SPI interface 
    /// DRIVER NOT COMPLETE
    /// </summary>
    public class Ssd1681 : EPaperTriColorBase
    {
        protected override bool IsBlackInverted => false;

        protected override bool IsColorInverted => false;

        public static byte[] LutData = { 0x02, 0x02, 0x01, 0x11, 0x12, 0x12 }; //""fiiYX\x99\x99\x88\x00\x00\x00\x00\xf8\xb4\x13Q5QQ\x19\x01\x00' };
                                                                               //_LUT_DATA = b'\x02\x02\x01\x11\x12\x12""fiiYX\x99\x99\x88\x00\x00\x00\x00\xf8\xb4\x13Q5QQ\x19\x01\x00'

        /// <summary>
        /// Create a new Ssd1681 object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Ssd1681(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width, int height) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
        {
        }

        /// <summary>
        /// Create a new Ssd1681 ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Ssd1681(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            IDigitalInputPort busyPort,
            int width, int height) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, busyPort, width, height)
        {
        }

        protected override void Initialize()
        {
            Reset();

            SendCommand(CommandSsd1681.SW_RESET);
            SendCommand(CommandSsd1681.DRIVER_CONTROL);
            SendData(new byte[] { (byte)(Width - 1), (byte)((Height - 1) >> 8), 0x0 });

            SendCommand(CommandSsd1681.DATA_MODE);
            SendData(0x03);

            SendCommand(CommandSsd1681.SET_RAMXPOS);
            SendData(new byte[] { 0x0, (byte)(Height / 8 - 1) });

            SendCommand(CommandSsd1681.SET_RAMYPOS);
            SendData(new byte[] { 0x0, 0x0, (byte)(Height - 1), (byte)((Height - 1) >> 8) });

            SendCommand(CommandSsd1681.WRITE_BORDER);
            SendData(0x05);

            SendCommand(CommandSsd1681.TEMP_CONTROL);
            SendData(0x80);

            WaitUntilIdle();
        }

        public override void Show()
        {
            SendCommand(CommandSsd1681.WRITE_BWRAM);
        }

        public override void Show(int left, int top, int right, int bottom)
        {   //ToDo check if this display supports partial updates (don't think it does)
            Show();
        }

        public void PowerDown()
        {
            SendCommand(CommandSsd1681.DEEP_SLEEP);
            SendData(0x01);
        }

        public void Update()
        {
            SendCommand(CommandSsd1681.DISP_CTRL2);
            SendData(0x07);
            SendCommand(CommandSsd1681.MASTER_ACTIVATE);
            WaitUntilIdle();
        }

        protected enum CommandSsd1681 : byte
        {
            DRIVER_CONTROL = 0x01,
            GATE_VOLTAGE = 0x03,
            SOURCE_VOLTAGE = 0x04,
            INIT_SETTING = 0x08,
            INIT_WRITE_REG = 0x09,
            INIT_READ_REG = 0x0A,
            BOOSTER_SOFT_START = 0x0C,
            DEEP_SLEEP = 0x10,
            DATA_MODE = 0x11,
            SW_RESET = 0x12,
            HV_DETECT = 0x14,
            VCI_DETECT = 0x15,
            TEMP_CONTROL = 0x18,
            TEMP_WRITE = 0x1A,
            TEMP_READ = 0x1B,
            EXTTEMP_WRITE = 0x1C,
            MASTER_ACTIVATE = 0x20,
            DISP_CTRL1 = 0x21,
            DISP_CTRL2 = 0x22,
            WRITE_BWRAM = 0x24,
            WRITE_REDRAM = 0x26,
            READ_RAM = 0x27,
            VCOM_SENSE = 0x28,
            VCOM_DURATION = 0x29,
            WRITE_VCOM_OTP = 0x2A,
            WRITE_VCOM_CTRL = 0x2B,
            WRITE_VCOM_REG = 0x2C,
            READ_OTP = 0x2D,
            READ_USERID = 0x2E,
            READ_STATUS = 0x2F,
            WRITE_WS_OTP = 0x30,
            LOAD_WS_OTP = 0x31,
            WRITE_LUT = 0x32,
            CRC_CALC = 0x34,
            CRC_READ = 0x35,
            PROG_OTP = 0x36,
            WRITE_DISPLAY_OPT = 0x37,
            WRITE_USERID = 0x38,
            OTP_PROGMODE = 0x39,
            WRITE_BORDER = 0x3C,
            END_OPTION = 0x3F,
            SET_RAMXPOS = 0x44,
            SET_RAMYPOS = 0x45,
            AUTOWRITE_RED = 0x46,
            AUTOWRITE_BW = 0x47,
            SET_RAMXCOUNT = 0x4E,
            SET_RAMYCOUNT = 0x4F,
            NOP = 0xFF,
        }

        void SendCommand(CommandSsd1681 command)
        {
            SendCommand((byte)command);
        }
    }
}