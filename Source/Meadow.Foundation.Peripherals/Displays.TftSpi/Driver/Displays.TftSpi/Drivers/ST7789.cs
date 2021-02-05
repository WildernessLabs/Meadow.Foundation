using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays.Tft
{
    public class St7789 : TftSpiBase
    {
        private byte xOffset;
        private byte yOffset;

        public override DisplayColorMode DefautColorMode => DisplayColorMode.Format12bppRgb444;

        public St7789(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width, int height, DisplayColorMode displayColorMode = DisplayColorMode.Format12bppRgb444) 
            : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height, displayColorMode)
        {
            Initialize();
        }

        protected override void Initialize()
        {
            resetPort.State = true;
            Thread.Sleep(50);
            resetPort.State = false;
            Thread.Sleep(50);
            resetPort.State = true;
            Thread.Sleep(50);

            if(width == 135)
            {   //unknown if this is consistant across all displays with this res
                xOffset = 52;
                yOffset = 40;
            }
            else
            {
                xOffset = yOffset = 0;
            }
            
            SendCommand(SWRESET);
            DelayMs(150);
            SendCommand(SLPOUT);
            DelayMs(500);

            SendCommand(COLOR_MODE);  // set color mode - 16 bit color (x55), 12 bit color (x53), 18 bit color (x56)
            if (ColorMode == DisplayColorMode.Format16bppRgb565)
                SendData(0x55);  // 16-bit color RGB565
            else
                SendData(0x53); //12-bit color RGB444
           
            DelayMs(10);

            SendCommand(MADCTL);
            SendData(0x00); //some variants use 0x08

            SendCommand((byte)LcdCommand.CASET);

            SendData(new byte[] { 0, 0, 0, (byte)Width });

            SendCommand((byte)LcdCommand.RASET);
            SendData(new byte[] { 0, 0, (byte)(Height >> 8), (byte)(Height & 0xFF) });

            SendCommand(INVON); //inversion on
            DelayMs(10);
            SendCommand(NORON); //normal display
            DelayMs(10);
            SendCommand(DISPON); //display on
            DelayMs(500);

            SetAddressWindow(0, 0, (width - 1), (height - 1));

            dataCommandPort.State = Data;
        }

        protected override void SetAddressWindow(int x0, int y0, int x1, int y1)
        {
            x0 += xOffset;
            y0 += yOffset;

            x1 += xOffset;
            y1 += yOffset;

            SendCommand((byte)LcdCommand.CASET);  // column addr set
            dataCommandPort.State = Data;
            Write((byte)(x0 >> 8));
            Write((byte)(x0 & 0xff));   // XSTART 
            Write((byte)(x1 >> 8));
            Write((byte)(x1 & 0xff));   // XEND

            SendCommand((byte)LcdCommand.RASET);  // row addr set
            dataCommandPort.State = Data;
            Write((byte)(y0 >> 8));
            Write((byte)(y0 & 0xff));    // YSTART
            Write((byte)(y1 >> 8));
            Write((byte)(y1 & 0xff));    // YEND

            SendCommand((byte)LcdCommand.RAMWR);  // write to RAM
        }

        public void SetRotation(Rotation rotation)
        {
            SendCommand(MADCTL);

            switch (rotation)
            {
                case Rotation.Normal:
                    SendData(MADCTL_MX | MADCTL_MY | MADCTL_RGB);
                    break;
                case Rotation.Rotate_90:
                    SendData(MADCTL_MY | MADCTL_MV | MADCTL_RGB);
                    break;
                case Rotation.Rotate_180:
                    SendData(MADCTL_RGB);
                    break;
                case Rotation.Rotate_270:
                    SendData(MADCTL_MX | MADCTL_MV | MADCTL_RGB);
                    break;
            }
        }

        static byte SWRESET = 0x01;
        static byte SLPOUT = 0x11;
        static byte NORON = 0x13;
        static byte INVON = 0x21;
        static byte DISPON = 0x29;
    }
}