using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents an SSD1680 ePaper color display
    /// </summary>
    public class Ssd1680 : EPaperTriColorBase
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
        /// Create a new Ssd1680 object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Ssd1680(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width, int height) :
            base(spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
        { }

        /// <summary>
        /// Create a new Ssd1680 ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Ssd1680(ISpiBus spiBus,
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

            WaitUntilIdle();
            SendCommand(0x12);  // SWRESET
            WaitUntilIdle();

            SendCommand(SSD1680_DRIVER_CONTROL);
            SendData(0xF9);
            SendData(0x00);
            SendData(0x00);

            SendCommand(SSD1680_DATA_MODE);
            SendData(0x03);

            //set window
            SendCommand(SSD1680_SET_RAMXPOS);
            SendData(0x00);
            SendData(Width / 8);

            SendCommand(SSD1680_SET_RAMYPOS);
            SendData(0x00);
            SendData(0x00);
            SendData(Height);
            SendData(Height >> 8);

            //set cursor
            SetRamAddress();

            SendCommand(SSD1680_WRITE_BORDER);
            SendData(0x05);

            SendCommand(SSD1680_TEMP_CONTROL);
            SendData(0x80);

            SendCommand(SSD1680_DISP_CTRL1);
            SendData(0x80);
            SendData(0x80);

            WaitUntilIdle();
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

            SendCommand(SSD1680_WRITE_RAM1);

            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(0xFF);
            }

            SetRamAddress();

            SendCommand(SSD1680_WRITE_RAM2);
            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(0xFF);
            }
        }

        void DisplayFrame(byte[] blackBuffer, byte[] colorBuffer)
        {
            SetRamAddress();
            SendCommand(SSD1680_WRITE_RAM1);
            SendData(blackBuffer);

            SetRamAddress();
            SendCommand(SSD1680_WRITE_RAM2);
            SendData(colorBuffer);

            DisplayFrame();
        }

        void DisplayFrame()
        {
            SendCommand(SSD1680_DISP_CTRL2);
            SendData(0xF4);

            SendCommand(SSD1680_MASTER_ACTIVATE);

            WaitUntilIdle();
        }

        void SetRamAddress()
        {
            SendCommand(SSD1680_SET_RAMXCOUNT);
            SendData(0x00);

            SendCommand(SSD1680_SET_RAMYCOUNT);
            SendData(0x00);
            SendData(0x00);
        }

        void PowerDown()
        {
            SendCommand(SSD1680_DEEP_SLEEP);
            SendData(0x01);

            DelayMs(100);
        }

        const byte SSD1680_DRIVER_CONTROL = 0x01;
        const byte SSD1680_GATE_VOLTAGE = 0x03;
        const byte SSD1680_SOURCE_VOLTAGE = 0x04;
        const byte SSD1680_PROGOTP_INITIAL = 0x08;
        const byte SSD1680_PROGREG_INITIAL = 0x09;
        const byte SSD1680_READREG_INITIAL = 0x0A;
        const byte SSD1680_BOOST_SOFTSTART = 0x0C;
        const byte SSD1680_DEEP_SLEEP = 0x10;
        const byte SSD1680_DATA_MODE = 0x11;
        const byte SSD1680_SW_RESET = 0x12;
        const byte SSD1680_TEMP_CONTROL = 0x18;
        const byte SSD1680_TEMP_WRITE = 0x1A;
        const byte SSD1680_MASTER_ACTIVATE = 0x20;
        const byte SSD1680_DISP_CTRL1 = 0x21;
        const byte SSD1680_DISP_CTRL2 = 0x22;
        const byte SSD1680_WRITE_RAM1 = 0x24;
        const byte SSD1680_WRITE_RAM2 = 0x26;
        const byte SSD1680_WRITE_VCOM = 0x2C;
        const byte SSD1680_READ_OTP = 0x2D;
        const byte SSD1680_READ_STATUS = 0x2F;
        const byte SSD1680_WRITE_LUT = 0x32;
        const byte SSD1680_WRITE_BORDER = 0x3C;
        const byte SSD1680_SET_RAMXPOS = 0x44;
        const byte SSD1680_SET_RAMYPOS = 0x45;
        const byte SSD1680_SET_RAMXCOUNT = 0x4E;
        const byte SSD1680_SET_RAMYCOUNT = 0x4F;
    }
}