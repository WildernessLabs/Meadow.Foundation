
using System.Threading;
using Meadow.Devices;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.ePaper
{
    //aka WaveShare EPD 2i9B
    /// <summary>
    ///     Represents an Il0373 ePaper color display
    ///     104x212, 2.13inch E-Ink three-color display, SPI interface 
    /// </summary>
    public class Il0373 : EpdColorBase
    {
        /// <summary>
        /// Create a new IL0373 object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Il0373(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width, int height) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
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
            SendData((byte)(Height & 0xFF));//width 128
            SendData((byte)(Width >> 8) & 0xFF);
            SendData((byte)(Width & 0xFF));
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
            SetPartialWindow(blackImageBuffer.Buffer, colorImageBuffer.Buffer,
                left, top, right - left, top - bottom);

            DisplayFrame();
        }

        public override void Show()
        {
            DisplayFrame(blackImageBuffer.Buffer, colorImageBuffer.Buffer);
        }

        //clear the frame data from the SRAM, this doesn't update the display
        protected void ClearFrame()
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
                SendData(colorBuffer[i]);
            }
            Thread.Sleep(2);

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