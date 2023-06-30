using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents an Ssd1681 ePaper B/W or color display commonly 1.54"
    /// 200x200, e-Ink three-color display, SPI interface 
    /// </summary>
    public class Ssd1681 : EPaperTriColorBase
    {
        /// <summary>
        /// Is black inverted on this display
        /// </summary>
        protected override bool IsBlackInverted => false;

        /// <summary>
        /// Is color inverted on this display
        /// </summary>
        protected override bool IsColorInverted => false;

        /// <summary>
        /// Create a new Ssd1681 object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Ssd1681(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width, int height) :
            base(spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
        { }

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
        { }

        /// <summary>
        /// Initialize the display
        /// </summary>
        protected override void Initialize()
        {
            Reset();

            DelayMs(100);

            SendCommand(SSD1681_SW_RESET);
            WaitUntilIdle();

            SendCommand(SSD1681_DATA_MODE);
            SendData(0x03);

            SendCommand(SSD1681_WRITE_BORDER);
            SendData(0x05);

            SendCommand(SSD1681_TEMP_CONTROL);
            SendData(0x80);

            SendCommand(SSD1681_SET_RAMXCOUNT);
            SendData(0x0);

            SendCommand(SSD1681_SET_RAMYCOUNT);
            SendData(0x0);
            SendData(0x0);

            SendCommand(SSD1681_DRIVER_CONTROL);
            SendData(Width - 1);
            SendData((Height - 1) >> 8);
            SendData(0x0);

            SetRamWindow();
        }


        /// <summary>
        /// Send the display buffer to the display and refresh
        /// </summary>
        public override void Show(int left, int top, int right, int bottom)
        {
            Show();
        }

        /// <summary>
        /// Send the display buffer to the display and refresh
        /// </summary>
        public override void Show()
        {
            DisplayFrame(imageBuffer.BlackBuffer, imageBuffer.ColorBuffer);
        }

        /// <summary>
        /// Clear the on-display frame buffer
        /// </summary>
        protected void ClearFrame()
        {
            SetRamAddress();

            SendCommand(SSD1681_WRITE_RAM1);

            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(0xFF);
            }

            SetRamAddress();

            SendCommand(SSD1681_WRITE_RAM2);
            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(0xFF);
            }
        }

        void DisplayFrame(byte[] blackBuffer, byte[] colorBuffer)
        {
            SetRamAddress();
            SendCommand(SSD1681_WRITE_RAM1);
            SendData(blackBuffer);

            SetRamAddress();
            SendCommand(SSD1681_WRITE_RAM2);

            dataCommandPort.State = DataState;

            for (int i = 0; i < colorBuffer.Length; i++)
            {   //invert the color data
                spiComms.Write((byte)~colorBuffer[i]);
            }

            DisplayFrame();
        }

        void DisplayFrame()
        {
            SendCommand(SSD1681_DISP_CTRL2);
            SendData(0xF7);
            SendCommand(SSD1681_MASTER_ACTIVATE);

            WaitUntilIdle();
        }

        void SetRamWindow()
        {
            SendCommand(SSD1681_SET_RAMXPOS);
            SendData(0);
            SendData(Width / 8 - 1);

            SendCommand(SSD1681_SET_RAMYPOS);
            SendData(0);
            SendData(Height - 1);
        }

        void SetRamAddress()
        {
            SendCommand(SSD1681_SET_RAMXCOUNT);
            SendData(0x00);

            SendCommand(SSD1681_SET_RAMYCOUNT);
            SendData(0x00);
            SendData(0x00);
        }

        void PowerDown()
        {
            SendCommand(SSD1681_DEEP_SLEEP);
            SendData(0x01);

            DelayMs(100);
        }

        const byte SSD1681_DRIVER_CONTROL = 0x01;
        const byte SSD1681_GATE_VOLTAGE = 0x03;
        const byte SSD1681_SOURCE_VOLTAGE = 0x04;
        const byte SSD1681_PROGOTP_INITIAL = 0x08;
        const byte SSD1681_PROGREG_INITIAL = 0x09;
        const byte SSD1681_READREG_INITIAL = 0x0A;
        const byte SSD1681_BOOST_SOFTSTART = 0x0C;
        const byte SSD1681_DEEP_SLEEP = 0x10;
        const byte SSD1681_DATA_MODE = 0x11;
        const byte SSD1681_SW_RESET = 0x12;
        const byte SSD1681_TEMP_CONTROL = 0x18;
        const byte SSD1681_TEMP_WRITE = 0x1A;
        const byte SSD1681_MASTER_ACTIVATE = 0x20;
        const byte SSD1681_DISP_CTRL1 = 0x21;
        const byte SSD1681_DISP_CTRL2 = 0x22;
        const byte SSD1681_WRITE_RAM1 = 0x24;
        const byte SSD1681_WRITE_RAM2 = 0x26;
        const byte SSD1681_WRITE_VCOM = 0x2C;
        const byte SSD1681_READ_OTP = 0x2D;
        const byte SSD1681_READ_STATUS = 0x2F;
        const byte SSD1681_WRITE_LUT = 0x32;
        const byte SSD1681_WRITE_BORDER = 0x3C;
        const byte SSD1681_SET_RAMXPOS = 0x44;
        const byte SSD1681_SET_RAMYPOS = 0x45;
        const byte SSD1681_SET_RAMXCOUNT = 0x4E;
        const byte SSD1681_SET_RAMYCOUNT = 0x4F;
    }
}