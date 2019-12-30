﻿using Meadow.Hardware;
using System.Threading;

namespace Meadow.Foundation.Displays.Tft
{
    public class Ili9163 : DisplayTftSpiBase
    {
        private Ili9163() { }

        public Ili9163(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            uint width, uint height) : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height)
        {
            Initialize();
        }
        
        protected override void Initialize()
        {
            resetPort.State = (true);
            Thread.Sleep(50);
            resetPort.State = (false);
            Thread.Sleep(50);
            resetPort.State = (true);
            Thread.Sleep(50);

            dataCommandPort.State = (Command); //software reset
            Write(0x01);

            dataCommandPort.State = (Command); //exit sleep mode
            Write(0x11);

            dataCommandPort.State = (Command); //set pixel format
            Write(0x3A);
            dataCommandPort.State = (Data);
            Write(0x05);//16 bit 565

            dataCommandPort.State = (Command);
            Write(0x26);
            dataCommandPort.State = (Data);
            Write(0x04);

            dataCommandPort.State = (Command);
            Write(0x36);
            dataCommandPort.State = (Data);
            Write(0x00); //set to RGB

            dataCommandPort.State = (Command);
            Write(0xF2);
            dataCommandPort.State = (Data);
            Write(0x01);

            dataCommandPort.State = (Command);
            Write(0xE0);
            dataCommandPort.State = (Data);
            Write(0x04);
            Write(0x3F);
            Write(0x25);
            Write(0x1C);
            Write(0x1E);
            Write(0x20);
            Write(0x12);
            Write(0x2A);
            Write(0x90);
            Write(0x24);
            Write(0x11);
            Write(0x00);
            Write(0x00);
            Write(0x00);
            Write(0x00);
            Write(0x00); // Positive Gamma

            dataCommandPort.State = (Command);
            Write(0xE1);
            dataCommandPort.State = (Data);
            Write(0x20);
            Write(0x20);
            Write(0x20);
            Write(0x20);
            Write(0x05);
            Write(0x00);
            Write(0x15);
            Write(0xA7);
            Write(0x3D);
            Write(0x18);
            Write(0x25);
            Write(0x2A);
            Write(0x2B);
            Write(0x2B);
            Write(0x3A); // Negative Gamma

            dataCommandPort.State = (Command);
            Write(0xB1);
            dataCommandPort.State = (Data);
            Write(0x08);
            Write(0x08); // Frame rate control 1

            dataCommandPort.State = (Command);
            Write(0xB4);
            dataCommandPort.State = (Data);
            Write(0x07);      // Display inversion

            dataCommandPort.State = (Command);
            Write(0xC0);
            dataCommandPort.State = (Data);
            Write(0x0A);
            Write(0x02); // Power control 1

            dataCommandPort.State = (Command);
            Write(0xC1);
            dataCommandPort.State = (Data);
            Write(0x02);       // Power control 2

            dataCommandPort.State = (Command);
            Write(0xC5);
            dataCommandPort.State = (Data);
            Write(0x50);
            Write(0x5B); // Vcom control 1

            dataCommandPort.State = (Command);
            Write(0xC7);
            dataCommandPort.State = (Data);
            Write(0x40);       // Vcom offset

            dataCommandPort.State = (Command);
            Write(0x2A);
            dataCommandPort.State = (Data);
            Write(0x00);
            Write(0x00);
            Write(0x00);
            Write(0x7F);
            Thread.Sleep(250); // Set column address

            dataCommandPort.State = (Command);
            Write(0x2B);
            dataCommandPort.State = (Data);
            Write(0x00);
            Write(0x00);
            Write(0x00);
            Write(0x9F);           // Set page address

            dataCommandPort.State = (Command);
            Write(0x36);
            dataCommandPort.State = (Data);
            Write(0xC0);       // Set address mode

            dataCommandPort.State = (Command);
            Write(0x29);           // Set display on
            Thread.Sleep(10);

            SetAddressWindow(0, 0, (width - 1), (height - 1));

            dataCommandPort.State = (Data);
        }

        protected override void SetAddressWindow(uint x0, uint y0, uint x1, uint y1)
        {
            SendCommand((byte)LcdCommand.CASET);  // column addr set
            dataCommandPort.State = Data;
            Write((byte)(x0 >> 8));
            Write((byte)(x0 & 0xff));   // XSTART 
            Write((byte)(x1 >> 8));
            Write((byte)(x1 & 0xff));   // XEND

            SendCommand((byte)LcdCommand.RASET);  // row addr set
            dataCommandPort.State = (Data);
            Write((byte)(y0 >> 8));
            Write((byte)(y0 & 0xff));    // YSTART
            Write((byte)(y1 >> 8));
            Write((byte)(y1 & 0xff));    // YEND

            dataCommandPort.State = (Command);
            Write((byte)LcdCommand.RAMWR);  // write to RAM */
        }
    }
}