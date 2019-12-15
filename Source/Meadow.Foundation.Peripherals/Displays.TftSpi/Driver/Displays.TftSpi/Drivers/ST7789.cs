using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays.Tft
{
    public class ST7789 : DisplayTftSpiBase
    {
        private byte xOffset;
        private byte yOffset;

        private ST7789() { }

        public ST7789(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            uint width, uint height) : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height)
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

            SendCommand(COLMOD);
            SendData(0x55); //16 bit color
            DelayMs(10);

            SendCommand(MADCTL);
            SendData(0x00); //some variants use 0x08

            SendCommand(CASET);

            SendData(new byte[] { 0, 0, 0, (byte)Width });

            SendCommand(RASET);
            SendData(new byte[] { 0, 0, (byte)(Height >> 8), (byte)(Height & 0xFF) });

            Console.WriteLine("Init display");

            SendCommand(INVON); //inversion on
            DelayMs(10);
            SendCommand(NORON); //normal display
            DelayMs(10);
            SendCommand(DISPON); //display on
            DelayMs(500);

            SetAddressWindow(0, 0, (width - 1), (height - 1));

            dataCommandPort.State = Data;
        }


        protected override void SetAddressWindow(uint x0, uint y0, uint x1, uint y1)
        {
            x0 += xOffset;
            y0 += yOffset;

            x1 += xOffset;
            y1 += yOffset;

            SendCommand(CASET);  // column addr set
            dataCommandPort.State = Data;
            Write((byte)(x0 >> 8));
            Write((byte)(x0 & 0xff));   // XSTART 
            Write((byte)(x1 >> 8));
            Write((byte)(x1 & 0xff));   // XEND

            SendCommand(RASET);  // row addr set
            dataCommandPort.State = Data;
            Write((byte)(y0 >> 8));
            Write((byte)(y0 & 0xff));    // YSTART
            Write((byte)(y1 >> 8));
            Write((byte)(y1 & 0xff));    // YEND

            SendCommand(RAMWR);  // write to RAM
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

        static byte XSTART = 0;
        static byte YSTART = 0;
        //static byte DELAY       = 0x80;    // special signifier for command lists
        //static byte NOP         = 0x00;
        static byte SWRESET = 0x01;
        //static byte RDDID       = 0x04;
        //static byte RDDST       = 0x09;
        //static byte SLPIN       = 0x10;
        static byte SLPOUT = 0x11;
        //static byte PTLON       = 0x12;
        static byte NORON = 0x13;
        //static byte INVOFF      = 0x20;
        static byte INVON = 0x21;
        //static byte DISPOFF     = 0x28;
        static byte DISPON = 0x29;
        static byte CASET = 0x2A;
        static byte RASET = 0x2B;
        static byte RAMWR = 0x2C;
        //static byte RAMRD       = 0x2E;
        //static byte PTLAR       = 0x30;
        static byte COLMOD = 0x3A;
        static byte MADCTL = 0x36;
        static byte MADCTL_MY = 0x80;
        static byte MADCTL_MX = 0x40;
        static byte MADCTL_MV = 0x20;
        //static byte MADCTL_ML   = 0x10;
        static byte MADCTL_RGB = 0x00;
        //static byte RDID1       = 0xDA;
        //static byte RDID2       = 0xDB;
        //static byte RDID3       = 0xDC;
        //static byte RDID4       = 0xDD;
    }
}