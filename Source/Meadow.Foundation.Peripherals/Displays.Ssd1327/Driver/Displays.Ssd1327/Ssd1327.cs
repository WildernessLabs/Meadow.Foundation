using System;
using System.Threading;
using Meadow.Devices;
using Meadow.Hardware;
using MicroGraphics.Buffers;

namespace Meadow.Foundation.Displays
{
    public partial class Ssd1327 : DisplayBase
    {
        public override DisplayColorMode ColorMode => DisplayColorMode.Format4bpp;

        public override int Width => 128;

        public override int Height => 128;

        protected ISpiPeripheral spiPeripheral;

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalOutputPort chipSelectPort;

        protected readonly BufferGray4 imageBuffer;

        protected const bool DataState = true;
        protected const bool CommandState = false;

        public Ssd1327(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin)
        {
            imageBuffer = new BufferGray4(Width, Height);

            dataCommandPort = device.CreateDigitalOutputPort(dcPin, false);
            if (resetPin != null) { resetPort = device.CreateDigitalOutputPort(resetPin, true); }
            if (chipSelectPin != null) { chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin, false); }

            spiPeripheral = new SpiPeripheral(spiBus, chipSelectPort);

            Initialize();

            Thread.Sleep(300);

            FillBuffer();
            Show();
        }

        int GetBufferLocation(int x, int y)
        {
            return (x / 2) + (y * 64);
        }

        public void SetContrast(byte contrast)
        {
            SendCommand(0x81);  //set contrast control
            SendCommand(contrast);  //Contrast byte
        }

        protected void Initialize()
        {
            Console.WriteLine("Initialize");

            dataCommandPort.State = CommandState;

            spiPeripheral.Write(init128x128);

            Thread.Sleep(100);              // 100ms delay recommended
            SendCommand(Command.SSD1327_DISPLAYON); // 0xaf
            SetContrast(0x2F);
        }

        public void FillBuffer()
        {
            for (int i = 0; i < imageBuffer.ByteCount; i++)
            {
                imageBuffer.Buffer[i] = 0xFF;
            }
        }

        public override void Clear(bool updateDisplay = false)
        {
            Array.Clear(imageBuffer.Buffer, 0, imageBuffer.ByteCount);

            if(updateDisplay == true)
            {
                Show();
            }
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color4bppGray);
        }

        public override void DrawPixel(int x, int y, bool colored)
        {
            DrawPixel(x, y, (byte)(colored ? 0x0F : 0));
        }

        public void DrawPixel(int x, int y, byte gray)
        {
            imageBuffer.SetPixel(x, y, gray);
        }

        public override void InvertPixel(int x, int y)
        {
            int index = GetBufferLocation(x, y);

            byte color = imageBuffer.GetPixelByte(x, y);

            color = (byte)(((byte)~color) & 0x0f);

            DrawPixel(x, y, color);
        }

        public override void Show(int left, int top, int right, int bottom)
        {
            // SetAddressWindow(left, top, (byte)(right - left), (byte)(top - bottom));
            //ToDo this should be pretty easy to implement with Memory<byte>
            Show();
        }

        public override void Show()
        {
            SetAddressWindow(0, 0, 127, 127);

            dataCommandPort.State = DataState;

            spiPeripheral.Write(imageBuffer.Buffer);
        }

        void SetAddressWindow(byte x0, byte y0, byte x1, byte y1)
        {
            SendCommand(Command.SSD1327_SETCOLUMN); //Set Column Address
            SendCommand(x0); //Beginning. Note that you must divide the column by 2, since 1 byte in memory is 2 pixels
            SendCommand((byte)(x1/2)); //End

            SendCommand(Command.SSD1327_SETROW); //Set Row Address
            SendCommand(y0); //Beginning
            SendCommand(y1); //End
        }

        protected void SendCommand(Command command)
        {
            SendCommand((byte)command);
        }

        protected void SendCommand(byte command)
        {
            dataCommandPort.State = CommandState;
            spiPeripheral.Write(command);
        }

        protected void SendData(int data)
        {
            SendData((byte)data);
        }

        protected void SendData(byte data)
        {
            dataCommandPort.State = DataState;
            spiPeripheral.Write(data);
        }

        protected void SendData(byte[] data)
        {
            dataCommandPort.State = DataState;
            spiPeripheral.Write(data);
        }

        // Init sequence, make sure its under 32 bytes, or split into multiples
        byte[] init128x128 = {
              // Init sequence for 128x32 OLED module
              (byte)Command.SSD1327_DISPLAYOFF, // 0xAE
              (byte)Command.SSD1327_SETCONTRAST,
              0x80,             // 0x81, 0x80
              (byte)Command.SSD1327_SEGREMAP, // 0xA0 0x53
              0x51, // remap memory, odd even columns, com flip and column swap
              (byte)Command.SSD1327_SETSTARTLINE,
              0x00, // 0xA1, 0x00
              (byte)Command.SSD1327_SETDISPLAYOFFSET,
              0x00, // 0xA2, 0x00
              (byte)Command.SSD1327_DISPLAYALLOFF, 
              (byte)Command.SSD1327_SETMULTIPLEX,
              0x7F, // 0xA8, 0x7F (1/64)
              (byte)Command.SSD1327_PHASELEN,
              0x11, // 0xB1, 0x11
              /*
              SSD1327_GRAYTABLE,
              0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
              0x07, 0x08, 0x10, 0x18, 0x20, 0x2f, 0x38, 0x3f,
              */
              (byte)Command.SSD1327_DCLK,
              0x00, // 0xb3, 0x00 (100hz)
              (byte)Command.SSD1327_REGULATOR,
              0x01, // 0xAB, 0x01
              (byte)Command.SSD1327_PRECHARGE2,
              0x04, // 0xB6, 0x04
              (byte)Command.SSD1327_SETVCOM,
              0x0F, // 0xBE, 0x0F
              (byte)Command.SSD1327_PRECHARGE,
              0x08, // 0xBC, 0x08
              (byte)Command.SSD1327_FUNCSELB,
              0x62, // 0xD5, 0x62
              (byte)Command.SSD1327_CMDLOCK,
              0x12, // 0xFD, 0x12
              (byte)Command.SSD1327_NORMALDISPLAY, 
              (byte)Command.SSD1327_DISPLAYON
        };
    }
}