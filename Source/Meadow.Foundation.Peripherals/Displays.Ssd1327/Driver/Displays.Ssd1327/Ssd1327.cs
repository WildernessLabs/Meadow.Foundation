using System;
using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    public class Ssd1327 : DisplayBase
    {
        public override DisplayColorMode ColorMode => DisplayColorMode.Format4bpp;

        public override int Width => 128;

        public override int Height => 128;

        protected SpiBus spiBus;
        protected ISpiPeripheral spiPeripheral;

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalOutputPort chipSelectPort;

        protected readonly byte[] spiBuffer;
        protected readonly byte[] spiReceive;

        protected byte currentPen;

        protected int xMin, xMax, yMin, yMax;

        protected const bool Data = true;
        protected const bool Command = false;

        public Ssd1327(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin)
        {
            this.spiBus = (SpiBus)spiBus;

            spiBuffer = new byte[Width * Height / 2]; 
            spiReceive = new byte[Width * Height / 2];

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

            dataCommandPort.State = Command;
            spiPeripheral.WriteBytes(init_128x128);

            Thread.Sleep(100);              // 100ms delay recommended
            SendCommand(SSD1327_DISPLAYON); // 0xaf
            SetContrast(0x2F);
        }

        public void FillBuffer()
        {
            for (int i = 0; i < spiBuffer.Length; i++)
            {
                spiBuffer[i] = 0xFF;
            }
        }

        public override void Clear(bool updateDisplay = false)
        {
            for(int i = 0; i < spiBuffer.Length; i++)
            {
                spiBuffer[i] = 0;
            }

            xMin = 0;
            yMin = 0;
            xMax = Width - 1;
            yMax = Height - 1;

            if(updateDisplay == true)
            {
                Show();
            }
        }

        byte GetGrayScaleFromColor(Color color)
        {   //0.21, 0.71, 0.071
            var gray = (byte)(53.55 * color.R + 181.05 * color.G + 18.105 * color.B);

            return (byte)(gray >> 4);
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, GetGrayScaleFromColor(color));
        }

        public override void DrawPixel(int x, int y, bool colored)
        {
            DrawPixel(x, y, (byte)(colored ? 0x0F : 0));
        }

        public override void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, currentPen);
        }

        public void DrawPixel(int x, int y, byte gray)
        {
            currentPen = gray;

            int index = GetBufferLocation(x, y);

            if ((x % 2) == 0)
            {   //even pixel - shift to the significant nibble
                spiBuffer[index] = (byte)((spiBuffer[index] & 0x0f) | (gray << 4));
            }
            else
            {   //odd pixel
                spiBuffer[index] = (byte)((spiBuffer[index] & 0xf0) | (gray));
            }
        }

        public override void InvertPixel(int x, int y)
        {
            int index = GetBufferLocation(x, y);

            byte color;

            if ((x % 2) == 0)
            {   //even pixel - shift to the significant nibble
                color = (byte)((spiBuffer[index] & 0x0f) >> 4);
            }
            else
            {   //odd pixel
                color = (byte)((spiBuffer[index] & 0xf0));
            }

            color = (byte)((color = (byte)~color) & 0x0f);

            DrawPixel(x, y, color);
        }

        public override void SetPenColor(Color color)
        {
            currentPen = GetGrayScaleFromColor(color);
        }

        public override void Show()
        {
            //  SetAddressWindow(0, 0, (byte)(Width - 1), (byte)(Height - 1));
            SetAddressWindow(0, 0, 127, 127);

            int len = spiBuffer.Length;

            dataCommandPort.State = Data;
            spiBus.ExchangeData(chipSelectPort, ChipSelectMode.ActiveLow, spiBuffer, spiReceive, len);

         /*   xMin = Width;
            yMin = Height;
            xMax = 0;
            yMax = 0; */
        }

        void SetAddressWindow(byte x0, byte y0, byte x1, byte y1)
        {
            SendCommand(SSD1327_SETCOLUMN); //Set Column Address
            SendCommand(x0); //Beginning. Note that you must divide the column by 2, since 1 byte in memory is 2 pixels
            SendCommand((byte)(x1/2)); //End

            SendCommand(SSD1327_SETROW); //Set Row Address
            SendCommand(y0); //Beginning
            SendCommand(y1); //End
        }

        protected void Write(byte value)
        {
            spiPeripheral.WriteByte(value);
        }

        protected void Write(byte[] data)
        {
            spiPeripheral.WriteBytes(data);
        }

        protected void SendCommand(byte command)
        {
            dataCommandPort.State = Command;
            Write(command);
        }

        protected void SendData(int data)
        {
            SendData((byte)data);
        }

        protected void SendData(byte data)
        {
            dataCommandPort.State = Data;
            Write(data);
        }

        protected void SendData(byte[] data)
        {
            dataCommandPort.State = Data;
            spiPeripheral.WriteBytes(data);
        }

        const byte SSD1327_BLACK = 0x0;
        const byte SSD1327_WHITE = 0xF;

        const byte SSD1327_I2C_ADDRESS = 0x3D;

        const byte SSD1305_SETBRIGHTNESS = 0x82;

        const byte SSD1327_SETCOLUMN = 0x15;

        const byte SSD1327_SETROW = 0x75;

        const byte SSD1327_SETCONTRAST = 0x81;

        const byte SSD1305_SETLUT = 0x91;

        const byte SSD1327_SEGREMAP = 0xA0;
        const byte SSD1327_SETSTARTLINE = 0xA1;
        const byte SSD1327_SETDISPLAYOFFSET = 0xA2;
        const byte SSD1327_NORMALDISPLAY = 0xA4;
        const byte SSD1327_DISPLAYALLON = 0xA5;
        const byte SSD1327_DISPLAYALLOFF = 0xA6;
        const byte SSD1327_INVERTDISPLAY = 0xA7;
        const byte SSD1327_SETMULTIPLEX = 0xA8;
        const byte SSD1327_REGULATOR = 0xAB;
        const byte SSD1327_DISPLAYOFF = 0xAE;
        const byte SSD1327_DISPLAYON = 0xAF;

        const byte SSD1327_PHASELEN = 0xB1;
        const byte SSD1327_DCLK = 0xB3;
        const byte SSD1327_PRECHARGE2 = 0xB6;
        const byte SSD1327_GRAYTABLE = 0xB8;
        const byte SSD1327_PRECHARGE = 0xBC;
        const byte SSD1327_SETVCOM = 0xBE;

        const byte SSD1327_FUNCSELB = 0xD5;

        const byte SSD1327_CMDLOCK = 0xFD;

        // Init sequence, make sure its under 32 bytes, or split into multiples
        byte[] init_128x128 = {
              // Init sequence for 128x32 OLED module
              SSD1327_DISPLAYOFF, // 0xAE
              SSD1327_SETCONTRAST,
              0x80,             // 0x81, 0x80
              SSD1327_SEGREMAP, // 0xA0 0x53
              0x51, // remap memory, odd even columns, com flip and column swap
              SSD1327_SETSTARTLINE,
              0x00, // 0xA1, 0x00
              SSD1327_SETDISPLAYOFFSET,
              0x00, // 0xA2, 0x00
              SSD1327_DISPLAYALLOFF, SSD1327_SETMULTIPLEX,
              0x7F, // 0xA8, 0x7F (1/64)
              SSD1327_PHASELEN,
              0x11, // 0xB1, 0x11
              /*
              SSD1327_GRAYTABLE,
              0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
              0x07, 0x08, 0x10, 0x18, 0x20, 0x2f, 0x38, 0x3f,
              */
              SSD1327_DCLK,
              0x00, // 0xb3, 0x00 (100hz)
              SSD1327_REGULATOR,
              0x01, // 0xAB, 0x01
              SSD1327_PRECHARGE2,
              0x04, // 0xB6, 0x04
              SSD1327_SETVCOM,
              0x0F, // 0xBE, 0x0F
              SSD1327_PRECHARGE,
              0x08, // 0xBC, 0x08
              SSD1327_FUNCSELB,
              0x62, // 0xD5, 0x62
              SSD1327_CMDLOCK,
              0x12, // 0xFD, 0x12
              SSD1327_NORMALDISPLAY, SSD1327_DISPLAYON
        };
    }
}