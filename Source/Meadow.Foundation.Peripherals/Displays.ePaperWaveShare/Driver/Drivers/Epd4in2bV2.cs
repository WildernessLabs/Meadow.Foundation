using Meadow.Hardware;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents an WaveShare Epd4in2b V2 ePaper color display
    /// 400x300, 4.2inch e-Ink three-color display, SPI interface 
    /// </summary>
    public class Epd4in2bV2 : EPaperTriColorBase
    {
        /// <summary>
        /// Create a new WaveShare Epd4in2b V2 400x300 pixel display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        public Epd4in2bV2(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin) :
            base(spiBus, chipSelectPin, dcPin, resetPin, busyPin, 400, 300)
        { }

        /// <summary>
        /// Create a new WaveShare Epd4in2b V2 ePaper 400x300 pixel display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        public Epd4in2bV2(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            IDigitalInputPort busyPort) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, busyPort, 400, 300)
        {
        }

        /// <summary>
        /// Does the display invert data for black pixels
        /// </summary>
        protected override bool IsBlackInverted => false;

        /// <summary>
        /// Does the display invert data for color pixels
        /// </summary>

        protected override bool IsColorInverted => false;

        /// <summary>
        /// Initialize the display
        /// </summary>
        protected override void Initialize()
        {
            Reset();

            SendCommand(Command.POWER_ON);
            WaitUntilIdle();
            SendCommand(Command.PANEL_SETTING);
            SendData(0x0F);
        }

        /// <summary>
        /// Reset the display
        /// </summary>
        protected override void Reset()
        {
            Resolver.Log.Info("Reset");

            resetPort.State = true;
            DelayMs(200);
            resetPort.State = false;
            DelayMs(2);
            resetPort.State = true;
            DelayMs(200);
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
            SendData(x >> 8);
            SendData(x & 0xf8);     // x should be the multiple of 8, the last 3 bit will always be ignored
            SendData(((x & 0xf8) + width - 1) >> 8);
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

            DelayMs(2);
            SendCommand(Command.DATA_START_TRANSMISSION_2);

            if (bufferColor != null)
            {
                for (int i = 0; i < width / 8 * height; i++)
                {
                    SendData(bufferColor[i]);
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
            SendData(x >> 8);
            SendData(x & 0xf8);     // x should be the multiple of 8, the last 3 bit will always be ignored
            SendData(((x & 0x1f8) + width - 1) >> 8);
            SendData(((x & 0x1f8) + width - 1) | 0x07);
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
            SendData(x >> 8);
            SendData(x & 0xf8);     // x should be the multiple of 8, the last 3 bit will always be ignored
            SendData(((x & 0x1f8) + width - 1) >> 8);
            SendData(((x & 0x1f8) + width - 1) | 0x07);
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

            DelayMs(2);
            SendData((byte)Command.PARTIAL_OUT);
        }

        /// <summary>
        /// Copy the display buffer to the display for a set region
        /// </summary>
        /// <param name="left">left bounds of region in pixels</param>
        /// <param name="top">top bounds of region in pixels</param>
        /// <param name="right">right bounds of region in pixels</param>
        /// <param name="bottom">bottom bounds of region in pixels</param>
        public override void Show(int left, int top, int right, int bottom)
        {
            SetPartialWindow(imageBuffer.BlackBuffer, imageBuffer.ColorBuffer,
                left, top, right - left, top - bottom);

            DisplayFrame();
        }

        /// <summary>
        /// Copy the display buffer to the display
        /// </summary>
        public override void Show()
        {
            DisplayFrame(imageBuffer.BlackBuffer, imageBuffer.ColorBuffer);
        }

        /// <summary>
        /// Clear the frame data from the SRAM, this doesn't update the display 
        /// </summary>
        protected virtual void ClearFrame()
        {
            SendCommand(Command.DATA_START_TRANSMISSION_1);
            Thread.Sleep(2);

            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(0xFF);
            }
            Thread.Sleep(2);

            SendCommand(Command.DATA_START_TRANSMISSION_2);
            Thread.Sleep(2);
            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(0xFF);
            }
            Thread.Sleep(2);
        }

        void DisplayFrame(byte[] blackBuffer, byte[] colorBuffer)
        {
            Resolver.Log.Info($"Display frame - width {Width}, height {Height}");

            SendCommand(Command.DATA_START_TRANSMISSION_1);
            Thread.Sleep(2);

            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(blackBuffer[i]);

            }
            Thread.Sleep(2);


            SendCommand(Command.DATA_START_TRANSMISSION_2);
            Thread.Sleep(2);
            for (int i = 0; i < Width * Height / 8; i++)
            {
                //SendData(0xFF); //white for clear, black for on
                SendData(colorBuffer[i]);
            }
            Thread.Sleep(2);

            DisplayFrame();
        }

        /// <summary>
        /// Send a refresh command to the display 
        /// Does not transfer new data
        /// </summary>
        public void DisplayFrame()
        {
            SendCommand(Command.DISPLAY_REFRESH);
            WaitUntilIdle();
        }

        /// <summary>
        /// Set the device to low power mode
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