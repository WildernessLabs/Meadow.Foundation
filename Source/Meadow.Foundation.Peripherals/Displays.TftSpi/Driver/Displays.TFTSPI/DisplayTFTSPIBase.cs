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
        protected uint yMax;

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
                Refresh();
            }
        }

        /// <summary>
        ///     Display a 1-bit bitmap
        /// 
        ///     This method simply calls a similar method in the display hardware.
        /// </summary>
        /// <param name="x">Abscissa of the top left corner of the bitmap.</param>
        /// <param name="y">Ordinate of the top left corner of the bitmap.</param>
        /// <param name="width">Width of the bitmap in bytes.</param>
        /// <param name="height">Height of the bitmap in bytes.</param>
        /// <param name="bitmap">Bitmap to display.</param>
        /// <param name="bitmapMode">How should the bitmap be transferred to the display?</param>
        public override void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, BitmapMode bitmapMode)
        {
            if ((width * height) != bitmap.Length)
            {
                throw new ArgumentException("Width and height do not match the bitmap size.");
            }

            for (var ordinate = 0; ordinate < height; ordinate++)
            {
                for (var abscissa = 0; abscissa < width; abscissa++)
                {
                    var b = bitmap[(ordinate * width) + abscissa];
                    byte mask = 0x01;

                    for (var pixel = 0; pixel < 8; pixel++)
                    {
                        DrawPixel(x + (8 * abscissa) + pixel, y + ordinate, (b & mask) > 0);
                        mask <<= 1;
                    }
                }
            }
        }

        /// <summary>
        ///     Display a 1-bit bitmap
        /// 
        ///     This method simply calls a similar method in the display hardware.
        /// </summary>
        /// <param name="x">Abscissa of the top left corner of the bitmap.</param>
        /// <param name="y">Ordinate of the top left corner of the bitmap.</param>
        /// <param name="width">Width of the bitmap in bytes.</param>
        /// <param name="height">Height of the bitmap in bytes.</param>
        /// <param name="bitmap">Bitmap to display.</param>
        /// <param name="color">The color of the bitmap.</param>
        public override void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, Color color)
        {
            if ((width * height) != bitmap.Length)
            {
                throw new ArgumentException("Width and height do not match the bitmap size.");
            }

            for (var ordinate = 0; ordinate < height; ordinate++)
            {
                for (var abscissa = 0; abscissa < width; abscissa++)
                {
                    var b = bitmap[(ordinate * width) + abscissa];
                    byte mask = 0x01;

                    for (var pixel = 0; pixel < 8; pixel++)
                    {
                        if ((b & mask) > 0)
                        {
                            DrawPixel(x + (8 * abscissa) + pixel, y + ordinate, color);
                        }
                        mask <<= 1;
                    }
                }
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

            yMax = (uint)Math.Max(yMax, y);
        }

        /// <summary>
        ///     Draw the display buffer to screen
        /// </summary>
        public void Refresh()
        {
            // spiDisplay.WriteBytes(spiBuffer);

            if (yMax == 0)
                return;

            SetAddressWindow(0, 0, Width - 1, yMax);

            int len = (int)((yMax + 1) * Width * 2);

            dataCommandPort.State = (Data);
            spi.ExchangeData(chipSelectPort, ChipSelectMode.ActiveLow, spiBuffer, spiReceive, len);
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

        public override void Show()
        {
            Refresh();
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