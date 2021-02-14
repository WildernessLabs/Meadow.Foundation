using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays.Tft
{
    public abstract class TftSpiBase : DisplayBase, IDisposable
    {
        protected const byte NO_OP = 0x0;
        protected const byte MADCTL = 0x36;
        protected const byte MADCTL_MY = 0x80;
        protected const byte MADCTL_MX = 0x40;
        protected const byte MADCTL_MV = 0x20;
        protected const byte MADCTL_ML = 0x10;
        protected const byte MADCTL_RGB = 0x00;
        protected const byte MADCTL_BGR = 0X08;
        protected const byte MADCTL_MH = 0x04;
        protected const byte MADCTL_SS = 0x02;
        protected const byte MADCTL_GS = 0x01;
        protected const byte COLOR_MODE = 0x3A;

        protected enum LcdCommand
        {
            CASET = 0x2A,
            RASET = 0x2B,
            RAMWR = 0x2C,
            RADRD = 0x2E
        };

        public enum Rotation
        {
            Normal, //zero
            Rotate_90, //in degrees
            Rotate_180,
            Rotate_270,
        }

        //these displays typically support 16 & 18 bit, some also include 8, 9, 12 and/or 24 bit color 

        public override DisplayColorMode ColorMode => colorMode;
        protected DisplayColorMode colorMode ;

        public abstract DisplayColorMode DefautColorMode { get; }
        public override int Width => width;
        public override int Height => height;

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalOutputPort chipSelectPort;
        protected SpiBus spi;
        protected ISpiPeripheral spiDisplay;

        protected byte[] spiBuffer;
        protected byte[] spiReceive;

        protected ushort currentPen;

        protected int width;
        protected int height;
        protected int xMin, xMax, yMin, yMax;

        protected const bool Data = true;
        protected const bool Command = false;

        protected abstract void Initialize();

        internal TftSpiBase()
        { }

        public TftSpiBase(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width, int height, DisplayColorMode mode = DisplayColorMode.Format16bppRgb565)
        {
            this.width = width;
            this.height = height;

            spi = (SpiBus)spiBus;

            dataCommandPort = device.CreateDigitalOutputPort(dcPin, false);
            if (resetPin != null) { resetPort = device.CreateDigitalOutputPort(resetPin, true); }
            if (chipSelectPin != null) { chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin, false); }

            spiDisplay = new SpiPeripheral(spiBus, chipSelectPort);

            SetColorMode(mode);
        }

        public virtual bool IsColorModeSupported(DisplayColorMode mode)
        {
            if(mode == DisplayColorMode.Format12bppRgb444 || 
                mode == DisplayColorMode.Format16bppRgb565)
            {
                return true;
            }
            return false;
        }

        public void SetColorMode(DisplayColorMode mode)
        {
            if(IsColorModeSupported(mode) == false)
            {
                throw new ArgumentException($"Mode {mode} not supported");
            }

            if(mode == DisplayColorMode.Format16bppRgb565)
            {
                spiBuffer = new byte[width * height * sizeof(ushort)];
                spiReceive = new byte[width * height * sizeof(ushort)];
            }
            else //Rgb444
            {
                spiBuffer = new byte[width * height * 3 / 2];
                spiReceive = new byte[width * height * 3 / 2];
            }
            colorMode = mode;
        }

        protected abstract void SetAddressWindow(int x0, int y0, int x1, int y1);

        /// <summary>
        ///     Clear the display.
        /// </summary>
        /// <param name="updateDisplay">Update the dipslay once the buffer has been cleared when true.</param>
        public override void Clear(bool updateDisplay = false)
        {
            Clear(0, updateDisplay);
        }

        public void Clear(Color color, bool updateDisplay = false)
        {
            Clear(GetUShortFromColor(color), updateDisplay);
        }

        protected void Clear(ushort color, bool updateDisplay = false)
        {
            ClearScreen(color);

            if (updateDisplay)
            {
                Show();
            }
        }

        /// <summary>
        ///     Sets the pen color used for DrawPixel calls
        ///     <param name="pen">Pen color</param>
        /// </summary>
        public override void SetPenColor(Color pen)
        {
            currentPen = GetUShortFromColor(pen);
        }

        /// <summary>
        ///     Draw a single pixel using the current pen
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        public override void DrawPixel(int x, int y)
        {
            SetPixel(x, y, currentPen);
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="colored">Turn the pixel on (true) or off (false).</param>
        public override void DrawPixel(int x, int y, bool colored)
        {
            //this works for now but it's a bit of a hack for 444
            SetPixel(x, y, (colored ? (ushort)(0xFFFF) : (ushort)0));
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="color">16bpp 5/6/5 or 4/4/4 ushort value for pixel color</param>
        public void DrawPixel(int x, int y, ushort color)
        {
            SetPixel(x, y, color);
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="color">Color of pixel.</param>
        public override void DrawPixel(int x, int y, Color color)
        {
            SetPixel(x, y, GetUShortFromColor(color));
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location</param>
        /// <param name="y">y location</param>
        /// <param name="r">8 bit red value</param>
        /// <param name="g">8 bit green value</param>
        /// <param name="b">8 bit blue value</param>
        public void DrawPixel(int x, int y, byte r, byte g, byte b)
        {
            SetPixel(x, y, GetColorFromRGB(r, g, b));
        }

        /// <summary>
        ///     Invert the color of a single pixel as represented in the display buffer
        /// </summary>
        /// <param name="x">x location</param>
        /// <param name="y">y location</param>
        public override void InvertPixel(int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
            { return; }

            if(colorMode == DisplayColorMode.Format16bppRgb565)
            {
                InvertPixelRgb565(x, y);
            }
            else
            {
                InvertPixelRgb444(x, y);
            }
        }

        void InvertPixelRgb565(int x, int y)
        {
            //get current color
            var index = ((y * width) + x) * sizeof(ushort);

            ushort color = (ushort)(spiBuffer[index] << 8| spiBuffer[++index]);

            //split into R,G,B & invert
            byte r = (byte)(0x1F - ((color >> 11) & 0x1F));
            byte g = (byte)(0x3F - ((color >> 5) & 0x3F));
            byte b = (byte)(0x1F - (color) & 0x1F);

            //get new color
            color = (ushort)(r << 11 | g << 5 | b);

            SetPixel565(x, y, color);
        }

        public void InvertPixelRgb444(int x, int y)
        {
            byte r, g, b;
            int index;
            if(x % 2 == 0)
            {
                index = (int)((x + y * Width) * 3 / 2);

                r = (byte)(spiBuffer[index] >> 4);
                g = (byte)(spiBuffer[index] & 0x0F);
                b = (byte)(spiBuffer[index + 1] >> 4);
            }
            else
            {
                index = (int)((x - 1 + y * Width) * 3 / 2) + 1;
                r = (byte)(spiBuffer[index] & 0x0F);
                g = (byte)(spiBuffer[index + 1] >> 4);
                b = (byte)(spiBuffer[index + 1] & 0x0F);
            }

            r = (byte)(~r & 0x0F);
            g = (byte)(~g & 0x0F);
            b = (byte)(~b & 0x0F);

            //get new color
            var color = (ushort)(r << 8 | g << 4 | b);

            SetPixel444(x, y, color);
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="color">16bpp (565) encoded color value</param>
        private void SetPixel565(int x, int y, ushort color)
        {
            var index = ((y * width) + x) * sizeof(ushort);

            spiBuffer[index] = (byte)(color >> 8);
            spiBuffer[++index] = (byte)(color);
        }

        private void SetPixel(int x, int y, ushort color)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
            { return; }

            if (colorMode == DisplayColorMode.Format16bppRgb565)
            {
                SetPixel565(x, y, color);
            }
            else
            {
                SetPixel444(x, y, color);
            }

            //will skip for now for performance 
            /*  xMin = Math.Min(xMin, x);
              xMax = Math.Max(xMax, x);
              yMin = Math.Min(yMin, y);
              yMax = Math.Max(yMax, y);  */
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="color">12bpp (444) encoded color value</param>
        private void SetPixel444(int x, int y, ushort color)
        {
            int index;
            //one of 2 possible write patterns 
            if(x % 2 == 0)
            {
                //1st byte RRRRGGGG
                //2nd byte BBBB
                index = (int)((x + y * Width) * 3 / 2);
                spiBuffer[index] = (byte)(color >> 4); //think this is correct - grab the r & g values
                index++;
                spiBuffer[index] = (byte)((spiBuffer[index] & 0x0F) | (color << 4));
            }
            else 
            {
                //1st byte     RRRR
                //2nd byte GGGGBBBB
                index = (int)((x - 1 + y * Width) * 3 / 2) + 1;
                spiBuffer[index] = (byte)((spiBuffer[index] & 0xF0) | (color >> 8));
                spiBuffer[++index] = (byte)color; //just the lower 8 bits
            }
        }

        /// <summary>
        ///     Draw the display buffer to screen
        /// </summary>
        public override void Show()
        {
            /*    if (xMax == 0 || yMax == 0)
                { return; }

                if(xMin > 0 || yMin > 0)
                {
                    Show(xMin, yMin, xMax, yMax);
                    return;
                } */

            //  SetAddressWindow(0, 0, Width - 1, yMax);
            SetAddressWindow(0, 0, Width - 1, Height);

            int len;
            if (colorMode == DisplayColorMode.Format16bppRgb565)
            { 
                len = (int)((Height + 1) * Width * 2);
            }
            else
            {
                len = (int)((Height + 1) * Width * 3 / 2);
            }

            dataCommandPort.State = Data;
            spi.ExchangeData(chipSelectPort, ChipSelectMode.ActiveLow, spiBuffer, spiReceive, len);

          /*  xMin = width;
            yMin = height;
            xMax = 0;
            yMax = 0; */
        }

        byte[] lineBufferSend;
        byte[] lineBufferReceive;
        /// <summary>
        ///     Draw the display buffer to screen from x0,y0 to x1,y1
        /// </summary>
        public void Show(int x0, int y0, int x1, int y1)
        {
            if(x1 < x0 || y1 < y0)
            {   //could throw an exception
                return;
            }

            if (lineBufferSend == null)
            {
                lineBufferSend = new byte[width * sizeof(ushort)];
                lineBufferReceive = new byte[width * sizeof(ushort)];
            }

            SetAddressWindow(x0, y0, x1, y1);

            var len = (x1 - x0 + 1) * sizeof(ushort);

            dataCommandPort.State = Data;

            int sourceIndex;
            for (int y = y0; y <= y1; y++)
            {
                sourceIndex = ((y * width) + x0) * sizeof(ushort);
                Array.Copy(spiBuffer, sourceIndex, lineBufferSend, 0, len);

                spi.ExchangeData(chipSelectPort, ChipSelectMode.ActiveLow, lineBufferSend, lineBufferReceive, (int)len);
            }

            xMin = width;
            yMin = height;
            xMax = 0;
            yMax = 0;
        }

        private ushort Get12BitColorFromRGB(byte red, byte green, byte blue)
        {
            red >>= 4;
            green >>= 4;
            blue >>= 4;

            return (ushort)(red << 8 | green << 4 | blue);
        }

        private ushort Get16BitColorFromRGB(byte red, byte green, byte blue)
        {
            red >>= 3;
            green >>= 2;
            blue >>= 3;

            return (ushort)(red << 11 | green << 5 | blue);
        }

        private ushort GetColorFromRGB(byte red, byte green, byte blue)
        {
            if (colorMode == DisplayColorMode.Format16bppRgb565)
            {
                return Get16BitColorFromRGB(red, green, blue);
            }
            else
            {
                return Get12BitColorFromRGB(red, green, blue);
            }
        }

        private ushort GetUShortFromColor(Color color)
        {
            //this seems heavy
            byte red = (byte)(color.R * 255.0);
            byte green = (byte)(color.G * 255.0);
            byte blue = (byte)(color.B * 255.0);

            return GetColorFromRGB(red, green, blue);
        }

        protected void Write(byte value)
        {
            spiDisplay.WriteByte(value);
        }

        protected void Write(byte[] data)
        {
            spiDisplay.WriteBytes(data);
        }

        protected void DelayMs(int millseconds)
        {
            Thread.Sleep(millseconds);
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
            spiDisplay.WriteBytes(data);
        }

        /// <summary>
        ///     Directly sets the display to a 16bpp color value
        /// </summary>
        /// <param name="color">16bpp color value (565)</param>
        public void ClearScreen(ushort color = 0)
        {
            var high = (byte)(color >> 8);
            var low = (byte)(color);

            var index = 0;
            spiBuffer[index++] = high;
            spiBuffer[index++] = low;
            spiBuffer[index++] = high;
            spiBuffer[index++] = low;
            spiBuffer[index++] = high;
            spiBuffer[index++] = low;
            spiBuffer[index++] = high;
            spiBuffer[index++] = low;
            spiBuffer[index++] = high;
            spiBuffer[index++] = low;
            spiBuffer[index++] = high;
            spiBuffer[index++] = low;
            spiBuffer[index++] = high;
            spiBuffer[index++] = low;
            spiBuffer[index++] = high;
            spiBuffer[index++] = low;

            Array.Copy(spiBuffer, 0, spiBuffer, 16, 16);
            Array.Copy(spiBuffer, 0, spiBuffer, 32, 32);
            Array.Copy(spiBuffer, 0, spiBuffer, 64, 64);
            Array.Copy(spiBuffer, 0, spiBuffer, 128, 128);
            Array.Copy(spiBuffer, 0, spiBuffer, 256, 256);

            index = 512;

            while (index < spiBuffer.Length - 256)
            {
                Array.Copy(spiBuffer, 0, spiBuffer, index, 256);
                index += 256;
            }

            while (index < spiBuffer.Length)
            {
                spiBuffer[index++] = high;
                spiBuffer[index++] = low;
            }

            /*
            xMin = 0;
            yMin = 0;
            xMax = Width - 1;
            yMax = Height - 1;
            */
        }

        public void Dispose()
        {
            spi = null;
            dataCommandPort = null;
            resetPort = null;
        }
    }
}