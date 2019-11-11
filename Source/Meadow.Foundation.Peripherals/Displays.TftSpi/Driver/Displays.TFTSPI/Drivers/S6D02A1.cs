using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays.Tft
{
    //Samsung S6D02A1 controller
    public class S6D02A1 : DisplayTftSpiBase
    {
        private S6D02A1() { }

        public S6D02A1(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
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

            SendCommand(0xf0, new byte[] { 0x5a, 0x5a });             // Excommand2
            SendCommand(0xfc, new byte[] { 0x5a, 0x5a });             // Excommand3
            SendCommand(0x26, new byte[] { 0x01 });                   // Gamma set
            SendCommand(0xfa, new byte[] { 0x02, 0x1f, 0x00, 0x10, 0x22, 0x30, 0x38, 0x3A, 0x3A, 0x3A, 0x3A, 0x3A, 0x3d, 0x02, 0x01 });   // Positive gamma control
            SendCommand(0xfb, new byte[] { 0x21, 0x00, 0x02, 0x04, 0x07, 0x0a, 0x0b, 0x0c, 0x0c, 0x16, 0x1e, 0x30, 0x3f, 0x01, 0x02 });   // Negative gamma control
            SendCommand(0xfd, new byte[] { 0x00, 0x00, 0x00, 0x17, 0x10, 0x00, 0x01, 0x01, 0x00, 0x1f, 0x1f });                           // Analog parameter control
            SendCommand(0xf4, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x3f, 0x3f, 0x07, 0x00, 0x3C, 0x36, 0x00, 0x3C, 0x36, 0x00 });   // Power control
            SendCommand(0xf5, new byte[] { 0x00, 0x70, 0x66, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x6d, 0x66, 0x06 });               // VCOM control
            SendCommand(0xf6, new byte[] { 0x02, 0x00, 0x3f, 0x00, 0x00, 0x00, 0x02, 0x00, 0x06, 0x01, 0x00 });                           // Source control
            SendCommand(0xf2, new byte[] { 0x00, 0x01, 0x03, 0x08, 0x08, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x04, 0x08, 0x08 });   //Display control
            SendCommand(0xf8, new byte[] { 0x11 });                   // Gate control
            SendCommand(0xf7, new byte[] { 0xc8, 0x20, 0x00, 0x00 });  // Interface control
            SendCommand(0xf3, new byte[] { 0x00, 0x00 });              // Power sequence control
            SendCommand(0x11, null);                 // Wake
            Thread.Sleep(150);
            SendCommand(0xf3, new byte[] { 0x00, 0x01 });  // Power sequence control
            Thread.Sleep(50);
            SendCommand(0xf3, new byte[] { 0x00, 0x03 });  // Power sequence control
            Thread.Sleep(50);
            SendCommand(0xf3, new byte[] { 0x00, 0x07 });  // Power sequence control
            Thread.Sleep(50);
            SendCommand(0xf3, new byte[] { 0x00, 0x0f });  // Power sequence control
            Thread.Sleep(150);
            SendCommand(0xf4, new byte[] { 0x00, 0x04, 0x00, 0x00, 0x00, 0x3f, 0x3f, 0x07, 0x00, 0x3C, 0x36, 0x00, 0x3C, 0x36, 0x00 });    // Power control
            Thread.Sleep(50);
            SendCommand(0xf3, new byte[] { 0x00, 0x1f });   // Power sequence control
            Thread.Sleep(50);
            SendCommand(0xf3, new byte[] { 0x00, 0x7f });   // Power sequence control
            Thread.Sleep(50);
            SendCommand(0xf3, new byte[] { 0x00, 0xff });   // Power sequence control
            Thread.Sleep(50);
            SendCommand(0xfd, new byte[] { 0x00, 0x00, 0x00, 0x17, 0x10, 0x00, 0x00, 0x01, 0x00, 0x16, 0x16 });                           // Analog parameter control
            SendCommand(0xf4, new byte[] { 0x00, 0x09, 0x00, 0x00, 0x00, 0x3f, 0x3f, 0x07, 0x00, 0x3C, 0x36, 0x00, 0x3C, 0x36, 0x00 });   // Power control
            SendCommand(0x36, new byte[] { 0xC8 }); // Memory access data control
            SendCommand(0x35, new byte[] { 0x00 }); // Tearing effect line on
            SendCommand(0x3a, new byte[] { 0x05 }); // Interface pixel control
            Thread.Sleep(150);
            SendCommand(0x29, null);                // Display on
            SendCommand(0x2c, null);				// Memory write

            SetAddressWindow(0, 0, (width - 1), (height - 1));

            dataCommandPort.State = (Data);
        }

        protected override void SetAddressWindow(uint x0, uint y0, uint x1, uint y1)
        {
            SendCommand((byte)LcdCommand.CASET);  // column addr set
            dataCommandPort.State = (Data);
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

        void SendCommand(byte command, byte[] data)
        {
            dataCommandPort.State = (Command);
            Write(command);

            if (data != null)
            {
                dataCommandPort.State = (Data);
                for (int i = 0; i < data.Length; i++)
                    Write(data[i]);
            }
        }
    }
}