using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.ePaper
{
    /// <summary>
    ///     Provide an interface to the WaveShare ePaper color displays
    ///     104x212, 2.13inch E-Ink display HAT for Raspberry Pi, three-color, SPI interface 
    /// </summary>
    public class EPD2i13b : EPDColorBase
    {
        public EPD2i13b(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin)
        { }

        public override uint Width => 104;
        public override uint Height => 212;

        protected override void Initialize()
        {
            Reset();

            SendCommand(BOOSTER_SOFT_START);
            SendData(0x17);
            SendData(0x17);
            SendData(0x17);
            SendCommand(POWER_ON);

            WaitUntilIdle();
            SendCommand(PANEL_SETTING);
            SendData(0x8F);
            SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
            SendData(0x37);
            SendCommand(RESOLUTION_SETTING);
            SendData(0x68);//width 104
            SendData(0x00);
            SendData(0xD4);//height 212
        }

        protected void SetPartialWindow(byte[] bufferBlack, byte[] bufferColor, int x, int y, int width, int height)
        {
            SendCommand(PARTIAL_IN);
            SendCommand(PARTIAL_WINDOW);
            SendData(x & 0xf8);     // x should be the multiple of 8, the last 3 bit will always be ignored
            SendData(((x & 0xf8) + width - 1) | 0x07);
            SendData(y >> 8);
            SendData(y & 0xff);
            SendData((y + height - 1) >> 8);
            SendData((y + height - 1) & 0xff);
            SendData(0x01);         // Gates scan both inside and outside of the partial window. (default) 
            DelayMs(2);
            SendCommand(DATA_START_TRANSMISSION_1);
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
            SendCommand(DATA_START_TRANSMISSION_2);

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
            SendCommand(PARTIAL_OUT);
        }

        protected void SetPartialWindowBlack(byte[] bufferBlack, int x, int y, int width, int height)
        {
            SendCommand(PARTIAL_IN);
            SendCommand(PARTIAL_WINDOW);
            SendData(x & 0xf8);     // x should be the multiple of 8, the last 3 bit will always be ignored
            SendData(((x & 0xf8) + width - 1) | 0x07);
            SendData(y >> 8);
            SendData(y & 0xff);
            SendData((y + height - 1) >> 8);
            SendData((y + height - 1) & 0xff);
            SendData(0x01);         // Gates scan both inside and outside of the partial window. (default) 
            DelayMs(2);
            SendCommand(DATA_START_TRANSMISSION_1);

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
            SendCommand(PARTIAL_OUT);
        }

        protected void SetPartialWindowColor(byte[] bufferColor, int x, int y, int width, int height)
        {
            SendCommand(PARTIAL_IN);
            SendCommand(PARTIAL_WINDOW);
            SendData(x & 0xf8);
            SendData(((x & 0xf8) + width - 1) | 0x07);
            SendData(y >> 8);
            SendData(y & 0xff);
            SendData((y + height - 1) >> 8);
            SendData((y + height - 1) & 0xff);
            SendData(0x01);         // Gates scan both inside and outside of the partial window. (default) 
            DelayMs(2);
            SendCommand(DATA_START_TRANSMISSION_2);

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
            SendData(PARTIAL_OUT);
        }

        protected override void Refresh()
        {
            xRefreshStart = -1;
            if (xRefreshStart == -1)
            {
                DisplayFrame(blackImageBuffer, colorImageBuffer);
            }
            else
            {
                SetPartialWindow(blackImageBuffer, colorImageBuffer,
                        xRefreshStart, yRefreshStart, xRefreshEnd - xRefreshStart, yRefreshEnd - yRefreshStart);

                DisplayFrame();
            }

            xRefreshStart = yRefreshStart = xRefreshEnd = yRefreshEnd = -1;
        }

        //clear the frame data from the SRAM, this doesn't update the display
        protected virtual void ClearFrame()
        {
            SendCommand(DATA_START_TRANSMISSION_1);
            Thread.Sleep(2);

            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(0xFF);
            }
            Thread.Sleep(2);

            SendCommand(DATA_START_TRANSMISSION_2);
            Thread.Sleep(2);
            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(0xFF);
            }
            Thread.Sleep(2);
        }

        void DisplayFrame(byte[] blackBuffer, byte[] colorBuffer)
        {
            SendCommand(DATA_START_TRANSMISSION_1);
            Thread.Sleep(2);

            for (int i = 0; i < Width * Height / 8; i++)
            {
                SendData(blackBuffer[i]);
            }
            Thread.Sleep(2);

            SendCommand(DATA_START_TRANSMISSION_2);
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
            SendCommand(DISPLAY_REFRESH);
            WaitUntilIdle();
        }

        protected virtual void Sleep()
        {
            SendCommand(POWER_OFF);
            WaitUntilIdle();
            SendCommand(DEEP_SLEEP);
            SendData(0xA5); //check code
        }
    }
}