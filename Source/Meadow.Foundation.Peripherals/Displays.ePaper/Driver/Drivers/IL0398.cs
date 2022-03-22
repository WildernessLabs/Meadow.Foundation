using Meadow.Devices;
using Meadow.Hardware;
using System.Threading;

namespace Meadow.Foundation.Displays.ePaper
{
    //aka WaveShare 4.2" B tri color
    /// <summary>
    ///     Represents an Il0398 ePaper color display
    ///     400x300, 4.2inch e-Ink three-color display, SPI interface 
    /// </summary>
    public class Il0398 : EpdColorBase
    {
        public Il0398(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width, int height) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
        {
        }

        protected override bool IsBlackInverted => false;
        protected override bool IsColorInverted => false;

        protected override void Initialize()
        {
            Reset();

            SendCommand(Command.BOOSTER_SOFT_START);
            SendData(0x17);
            SendData(0x17);
            SendData(0x17);
            SendCommand(Command.POWER_ON);

            WaitUntilIdle();
            SendCommand(0x0F);

            SendCommand(Command.RESOLUTION_SETTING);
            SendData((byte)(Height >> 8) & 0xFF);
            SendData((byte)(Height & 0xFF));
            SendData((byte)(Width >> 8) & 0xFF);
            SendData((byte)(Width & 0xFF));
        }

        protected void SetPartialWindow(byte[] bufferBlack, byte[] bufferColor, int x, int y, int width, int height)
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