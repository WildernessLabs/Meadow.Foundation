using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays.Tft
{
    public abstract class DisplayTftSpiBase : DisplayBase, IDisposable
    {
        #region Enums
        protected enum LcdCommand
        {
            CASET = 0x2A,
            RASET = 0x2B,
            RAMWR = 0x2C
        };

        public enum Rotation
        {
            Normal, //zero
            Rotate_90, //in degrees
            Rotate_180,
            Rotate_270,
        }

        #endregion

        //these displays typically support 12, 16 & 18 bit but the current driver only supports 16
        public override DisplayColorMode ColorMode => DisplayColorMode.Format16bppRgb565;
        public override uint Width => width;
        public override uint Height => height;

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalOutputPort chipSelectPort;
        protected SpiBus spi;
        protected ISpiPeripheral spiDisplay;

        protected readonly byte[] spiBuffer;
        protected readonly byte[] spiReceive;

        protected ushort currentPen;

        protected uint width;
        protected uint height;
        protected uint xMin, xMax, yMin, yMax;

        protected const bool Data = true;
        protected const bool Command = false;

        protected abstract void Initialize();

        internal DisplayTftSpiBase()
        {
        }

        public DisplayTftSpiBase(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            uint width, uint height)
        {
            this.width = width;
            this.height = height;

            spi = (SpiBus)spiBus;

            spiBuffer = new byte[this.width * this.height * sizeof(ushort)];
            spiReceive = new byte[this.width * this.height * sizeof(ushort)];

            dataCommandPort = device.CreateDigitalOutputPort(dcPin, false);
            if (resetPin != null) { resetPort = device.CreateDigitalOutputPort(resetPin, true); }
            if (chipSelectPin != null) { chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin, false); }

            spiDisplay = new SpiPeripheral(spiBus, chipSelectPort);
        }

        protected abstract void SetAddressWindow(uint x0, uint y0, uint x1, uint y1);

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
            Clear(Get16BitColorFromColor(color), updateDisplay);
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
            currentPen = Get16BitColorFromColor(pen);
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
            SetPixel(x, y, (colored ? (ushort)(0xFFFF) : (ushort)0));
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="color">16bpp 5/6/5 ushort value for pixel color</param>
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
            SetPixel(x, y, Get16BitColorFromColor(color));
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="r">8 bit red value</param>
        /// <param name="g">8 bit green value</param>
        /// <param name="b">8 bit blue value</param>
        public void DrawPixel(int x, int y, byte r, byte g, byte b)
        {
            SetPixel(x, y, Get16BitColorFromRGB(r, g, b));
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="color">16bpp (565) encoded color value</param>
        private void SetPixel(int x, int y, ushort color)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return;

            var index = ((y * width) + x) * sizeof(ushort);

            spiBuffer[index] = (byte)(color >> 8);
            spiBuffer[++index] = (byte)(color);

            xMin = (uint)Math.Min(xMin, x);
            xMax = (uint)Math.Max(xMax, x);
            yMin = (uint)Math.Min(yMin, y);
            yMax = (uint)Math.Max(yMax, y);
        }

        /// <summary>
        ///     Draw the display buffer to screen
        /// </summary>
        public override void Show()
        {
            // spiDisplay.WriteBytes(spiBuffer);

            if (xMax == 0 || yMax == 0)
                return;

            if(xMin > 0 || yMin > 0)
            {
                Show(xMin, yMin, xMax, yMax);
                return;
            }

            SetAddressWindow(0, 0, Width - 1, yMax);

            int len = (int)((yMax + 1) * Width * 2);

            dataCommandPort.State = Data;
            spi.ExchangeData(chipSelectPort, ChipSelectMode.ActiveLow, spiBuffer, spiReceive, len);

            xMin = width;
            yMin = height;
            xMax = 0;
            yMax = 0;
        }


        byte[] lineBufferSend;
        byte[] lineBufferReceive;
        /// <summary>
        ///     Draw the display buffer to screen from x0,y0 to x1,y1
        /// </summary>
        public void Show(uint x0, uint y0, uint x1, uint y1)
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

            uint sourceIndex;
            for (uint y = y0; y <= y1; y++)
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

        private ushort Get16BitColorFromRGB(byte red, byte green, byte blue)
        {
            red >>= 3;
            green >>= 2;
            blue >>= 3;

            return (ushort)(red << 11 | green << 5 | blue);
        }

        private ushort Get16BitColorFromColor(Color color)
        {
            //this seems heavy
            byte red = (byte)(color.R * 255.0);
            byte green = (byte)(color.G * 255.0);
            byte blue = (byte)(color.B * 255.0);

            return Get16BitColorFromRGB(red, green, blue);
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

            xMin = 0;
            yMin = 0;
            xMax = Width - 1;
            yMax = Height - 1;
        }

        /*
         * from Netduino testing, can safely remove
        public void ClearWithoutFullScreenBuffer(ushort color)
        {
            var buffer = new ushort[_width];

            for (int x = 0; x < _width; x++)
            {
                buffer[x] = color;
            }

            for (int y = 0; y < _height; y++)
            {
                spiDisplay.WriteBytes(buffer);
            }
        }*/

        public void Dispose()
        {
            spi = null;
            dataCommandPort = null;
            resetPort = null;
        }
    }
}