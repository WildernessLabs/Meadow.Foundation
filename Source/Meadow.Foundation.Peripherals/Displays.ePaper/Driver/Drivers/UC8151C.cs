using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents an Uc8151c ePaper color display
    /// 152x152, 1.54inch E-Ink three-color display, SPI interface 
    /// </summary>
    public class Uc8151c : EPaperTriColorBase
    {
        /// <summary>
        /// Create a new Uc8151c object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Uc8151c(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width, int height) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
        { }

        /// <summary>
        /// Create a new Uc8151c ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Uc8151c(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            IDigitalInputPort busyPort,
            int width, int height) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, busyPort, width, height)
        {
        }

        /// <summary>
        /// Is black inverted on this display
        /// </summary>
        protected override bool IsBlackInverted => false;

        /// <summary>
        /// Is color inverted on this display
        /// </summary>
        protected override bool IsColorInverted => false;

        /// <summary>
        /// Initialize the display
        /// </summary>
        protected override void Initialize()
        {
            Reset();

            SendCommand(Command.BOOSTER_SOFT_START);
            SendData(0x17);
            SendData(0x17);
            SendData(0x17);

            SendCommand(Command.POWER_SETTING);
            SendData(0x03);
            SendData(0x00);
            SendData(0x2B);
            SendData(0x2B);
            SendData(0x09);

            SendCommand(Command.POWER_ON);
            WaitUntilIdle();

            SendCommand(Command.PANEL_SETTING);
            SendData(0xDF);

            SendCommand(Command.RESOLUTION_SETTING);
            SendData((byte)(Width & 0xFF));
            SendData((byte)(Height >> 8) & 0xFF);
            SendData((byte)(Height & 0xFF));

            SendCommand(Command.VCOM_AND_DATA_INTERVAL_SETTING);
            SendData(0xF7);

            SendCommand(Command.VCM_DC_SETTING);
            SendData(0x0A);
        }

        protected void SetPartialWindow(byte[] bufferBlack, byte[] bufferColor, int x, int y, int width, int height)
        {
            SendCommand(Command.PARTIAL_IN);
            SendCommand(Command.PARTIAL_WINDOW);
            SendData(x & 0xf8);     // x should be the multiple of 8, the last 3 bit will always be ignored
            SendData(((x & 0xf8) + width - 1) | 0x07);
            SendData(y >> 8);
            SendData(y & 0xff);
            SendData((y + height - 1) >> 8);
            SendData((y + height - 1) & 0xff);
            SendData(0x01);         // Gates scan both inside and outside of the partial window. (default) 
            DelayMs(2);
            SendCommand(Command.DATA_START_TRANSMISSION_1);

            dataCommandPort.State = DataState;

            if (bufferBlack != null)
            {
                for (int i = 0; i < width / 8 * height; i++)
                {
                    spiPeripheral.Write(bufferBlack[i]);
                }
            }
            else
            {
                for (int i = 0; i < width / 8 * height; i++)
                {
                    spiPeripheral.Write(0x00);
                }
            }
            DelayMs(2);
            SendCommand(Command.DATA_START_TRANSMISSION_2);

            dataCommandPort.State = DataState;

            if (bufferColor != null)
            {
                for (int i = 0; i < width / 8 * height; i++)
                {
                    spiPeripheral.Write(bufferColor[i]);
                }
            }
            else
            {
                for (int i = 0; i < width / 8 * height; i++)
                {
                    spiPeripheral.Write(0x00);
                }
            }
            DelayMs(2);
            SendCommand(Command.PARTIAL_OUT);
        }

        protected void SetPartialWindowBlack(byte[] bufferBlack, int x, int y, int width, int height)
        {
            SendCommand(Command.PARTIAL_IN);
            SendCommand(Command.PARTIAL_WINDOW);
            SendData(x & 0xf8);     // x should be the multiple of 8, the last 3 bit will always be ignored
            SendData(((x & 0xf8) + width - 1) | 0x07);
            SendData(y >> 8);
            SendData(y & 0xff);
            SendData((y + height - 1) >> 8);
            SendData((y + height - 1) & 0xff);
            SendData(0x01);         // Gates scan both inside and outside of the partial window. (default) 
            DelayMs(2);
            SendCommand(Command.DATA_START_TRANSMISSION_1);

            if (bufferBlack != null)
            {
                for (int i = 0; i < width / 8 * height; i++)
                {
                    SendData(bufferBlack[i]);
                }
            }
            else
            {
                for (int i = 0; i < width / 8 * height; i++)
                {
                    SendData(0x00);
                }
            }

            DelayMs(2);
            SendCommand(Command.PARTIAL_OUT);
        }

        protected void SetPartialWindowColor(byte[] bufferColor, int x, int y, int width, int height)
        {
            SendCommand(Command.PARTIAL_IN);
            SendCommand(Command.PARTIAL_WINDOW);
            SendData(x & 0xf8);
            SendData(((x & 0xf8) + width - 1) | 0x07);
            SendData(y >> 8);
            SendData(y & 0xff);
            SendData((y + height - 1) >> 8);
            SendData((y + height - 1) & 0xff);
            SendData(0x01);         // Gates scan both inside and outside of the partial window. (default) 
            DelayMs(2);
            SendCommand(Command.DATA_START_TRANSMISSION_2);

            if (bufferColor != null)
            {
                for (int i = 0; i < width / 8 * height; i++)
                {
                    SendData(bufferColor[i]);
                }
            }
            else
            {
                for (int i = 0; i < width / 8 * height; i++)
                {
                    SendData(0x00);
                }
            }

            DelayMs(2);
            SendData((byte)Command.PARTIAL_OUT);
        }

        public override void Show(int left, int top, int right, int bottom)
        {
            SetPartialWindow(imageBuffer.BlackBuffer, imageBuffer.ColorBuffer,
                left, top, right - left, top - bottom);

            DisplayFrame();
        }

        public override void Show()
        {
            DisplayFrame(imageBuffer.BlackBuffer, imageBuffer.ColorBuffer);
        }

        //clear the frame data from the SRAM, this doesn't update the display
        protected void ClearFrame()
        {
            SendCommand(Command.DATA_START_TRANSMISSION_1);
            DelayMs(2);

            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(0xFF);
            }
            DelayMs(2);

            SendCommand(Command.DATA_START_TRANSMISSION_2);
            DelayMs(2);
            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(0xFF);
            }
            DelayMs(2);
        }

        void DisplayFrame(byte[] blackBuffer, byte[] colorBuffer)
        {
            SendCommand(Command.DATA_START_TRANSMISSION_1);
            DelayMs(2);

            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(blackBuffer[i]);
            }
            DelayMs(2);

            SendCommand(Command.DATA_START_TRANSMISSION_2);
            DelayMs(2);
            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(colorBuffer[i]);
            }
            DelayMs(2);

            DisplayFrame();
        }

        public void DisplayFrame()
        {
            SendCommand(Command.DISPLAY_REFRESH);
            WaitUntilIdle();
        }

        protected virtual void Sleep()
        {
            SendCommand(Command.POWER_OFF);
            WaitUntilIdle();
            SendCommand(Command.DEEP_SLEEP);
            SendData(0xA5); //check code
        }
    }
}