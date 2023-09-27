using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents an Il0373 ePaper color display
    /// 104x212, 2.13inch E-Ink three-color display, SPI interface 
    /// </summary>
    public class Il0373 : EPaperTriColorBase
    {
        /// <summary>
        /// Create a new IL0373 object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Il0373(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width, int height) :
            base(spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
        { }

        /// <summary>
        /// Create a new Il0373 ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Il0373(ISpiBus spiBus,
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
            SendCommand(Command.POWER_ON);

            WaitUntilIdle();
            SendCommand(Command.PANEL_SETTING);
            SendData(0xCF);
            SendCommand(Command.VCOM_AND_DATA_INTERVAL_SETTING);
            SendData(0x77);
            SendCommand(Command.RESOLUTION_SETTING);
            SendData((byte)(Width & 0xFF));
            SendData((byte)(Height >> 8) & 0xFF);
            SendData((byte)(Height & 0xFF));
            SendCommand(Command.VCM_DC_SETTING);
            SendData(0x0A);
        }

        /// <summary>
        /// Set partial window for display updates
        /// </summary>
        /// <param name="bufferBlack">The buffer with black pixel data</param>
        /// <param name="bufferColor">The buffer with color pixel data</param>
        /// <param name="x">The x start position in pixels</param>
        /// <param name="y">The y start position in pixels</param>
        /// <param name="width">The width to update in pixels</param>
        /// <param name="height">The height to update in pixels</param>
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
                    spiComms.Write(bufferBlack[i]);
                }
            }
            else
            {
                for (int i = 0; i < width / 8 * height; i++)
                {
                    spiComms.Write(0x00);
                }
            }
            DelayMs(2);
            SendCommand(Command.DATA_START_TRANSMISSION_2);

            dataCommandPort.State = DataState;

            if (bufferColor != null)
            {
                for (int i = 0; i < width / 8 * height; i++)
                {
                    spiComms.Write(bufferColor[i]);
                }
            }
            else
            {
                for (int i = 0; i < width / 8 * height; i++)
                {
                    spiComms.Write(0x00);
                }
            }
            DelayMs(2);
            SendCommand(Command.PARTIAL_OUT);
        }

        /// <summary>
        /// Set partial window for display updates
        /// </summary>
        /// <param name="bufferBlack">The buffer with black pixel data</param>
        /// <param name="x">The x start position in pixels</param>
        /// <param name="y">The y start position in pixels</param>
        /// <param name="width">The width to update in pixels</param>
        /// <param name="height">The height to update in pixels</param>
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

        /// <summary>
        /// Set partial window for display updates
        /// </summary>
        /// <param name="bufferColor">The buffer with color pixel data</param>
        /// <param name="x">The x start position in pixels</param>
        /// <param name="y">The y start position in pixels</param>
        /// <param name="width">The width to update in pixels</param>
        /// <param name="height">The height to update in pixels</param>
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

        /// <summary>
        /// Update a region of the display from the offscreen buffer
        /// </summary>
        /// <param name="left">Left bounds in pixels</param>
        /// <param name="top">Top bounds in pixels</param>
        /// <param name="right">Right bounds in pixels</param>
        /// <param name="bottom">Bottom bounds in pixels</param>
        public override void Show(int left, int top, int right, int bottom)
        {
            SetPartialWindow(imageBuffer.BlackBuffer, imageBuffer.ColorBuffer,
                left, top, right - left, top - bottom);

            DisplayFrame();
        }

        /// <summary>
        /// Update the display from the offscreen buffer
        /// </summary>
        public override void Show()
        {
            DisplayFrame(imageBuffer.BlackBuffer, imageBuffer.ColorBuffer);
        }

        /// <summary>
        /// Clears the SRAM on the display controller
        /// Doesn't update the display
        /// </summary>
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

        /// <summary>
        /// Display data from the display controller SRAM
        /// </summary>
        public void DisplayFrame()
        {
            SendCommand(Command.DISPLAY_REFRESH);
            WaitUntilIdle();
        }

        /// <summary>
        /// Set the display to sleep state
        /// </summary>
        protected virtual void Sleep()
        {
            SendCommand(Command.POWER_OFF);
            WaitUntilIdle();
            SendCommand(Command.DEEP_SLEEP);
            SendData(0xA5); //check code
        }
    }
}