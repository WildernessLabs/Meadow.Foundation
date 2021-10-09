using Meadow.Devices;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays.TftSpi
{
    public abstract partial class TftSpiBase : DisplayBase, IDisposable
    {
        //these displays typically support 16 & 18 bit, some also include 8, 9, 12 and/or 24 bit color 

        public override DisplayColorMode ColorMode => colorMode;
        protected DisplayColorMode colorMode;

        public abstract DisplayColorMode DefautColorMode { get; }
        public override int Width => width;
        public override int Height => height;

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalOutputPort chipSelectPort;
        protected ISpiBus spi;
        protected ISpiPeripheral spiDisplay;

        protected Memory<byte> writeBuffer;
        protected Memory<byte> readBuffer;

        protected int width;
        protected int height;
        protected int xMin, xMax, yMin, yMax;

        protected const bool Data = true;
        protected const bool Command = false;

        protected abstract void Initialize();

        internal TftSpiBase()
        { }

        public TftSpiBase(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width, int height, DisplayColorMode mode = DisplayColorMode.Format16bppRgb565)
        {
            this.width = width;
            this.height = height;

            spi = spiBus;

            dataCommandPort = device.CreateDigitalOutputPort(dcPin, false);
            if (resetPin != null) { resetPort = device.CreateDigitalOutputPort(resetPin, true); }
            if (chipSelectPin != null) { chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin, false); }

            spiDisplay = new SpiPeripheral(spiBus, chipSelectPort);

            SetColorMode(mode);
        }

        public virtual bool IsColorModeSupported(DisplayColorMode mode)
        {
            if (mode == DisplayColorMode.Format12bppRgb444 ||
                mode == DisplayColorMode.Format16bppRgb565)
            {
                return true;
            }
            return false;
        }

        public void SetColorMode(DisplayColorMode mode)
        {
            if (IsColorModeSupported(mode) == false)
            {
                throw new ArgumentException($"Mode {mode} not supported");
            }

            if (mode == DisplayColorMode.Format16bppRgb565)
            {
                writeBuffer = new byte[width * height * sizeof(ushort)];
                readBuffer = new byte[width * height * sizeof(ushort)];
            }
            else //Rgb444
            {
                writeBuffer = new byte[width * height * 3 / 2];
                readBuffer = new byte[width * height * 3 / 2];
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
            //Array.Clear(spiBuffer, 0, spiBuffer.Length);
            writeBuffer.Span.Clear();

            if (updateDisplay) { Show(); }
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
            SetPixel(x, y, GetUShortFromColor(new Color(r, g, b)));
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

            ushort color = (ushort)(writeBuffer.Span[index] << 8| writeBuffer.Span[++index]);

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

                r = (byte)(writeBuffer.Span[index] >> 4);
                g = (byte)(writeBuffer.Span[index] & 0x0F);
                b = (byte)(writeBuffer.Span[index + 1] >> 4);
            }
            else
            {
                index = (int)((x - 1 + y * Width) * 3 / 2) + 1;
                r = (byte)(writeBuffer.Span[index] & 0x0F);
                g = (byte)(writeBuffer.Span[index + 1] >> 4);
                b = (byte)(writeBuffer.Span[index + 1] & 0x0F);
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

            writeBuffer.Span[index] = (byte)(color >> 8);
            writeBuffer.Span[++index] = (byte)(color);
        }

        private void SetPixel(int x, int y, ushort color)
        {
          //  if (x < 0 || y < 0 || x >= width || y >= height)
          //  { return; }

            if (colorMode == DisplayColorMode.Format16bppRgb565)
            {
                SetPixel565(x, y, color);
            }
            else
            {
                SetPixel444(x, y, color);
            }
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
                writeBuffer.Span[index] = (byte)(color >> 4); //think this is correct - grab the r & g values
                index++;
                writeBuffer.Span[index] = (byte)((writeBuffer.Span[index] & 0x0F) | (color << 4));
            }
            else 
            {
                //1st byte     RRRR
                //2nd byte GGGGBBBB
                index = ((x - 1 + y * Width) * 3 / 2) + 1;
                writeBuffer.Span[index] = (byte)((writeBuffer.Span[index] & 0xF0) | (color >> 8));
                writeBuffer.Span[++index] = (byte)color; //just the lower 8 bits
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
                len = ((Height + 1) * Width * 2);
            }
            else
            {
                len = ((Height + 1) * Width * 3 / 2);
            }

            dataCommandPort.State = Data;
            spi.Exchange(chipSelectPort, writeBuffer.Span, readBuffer.Span);

          /*  xMin = width;
            yMin = height;
            xMax = 0;
            yMax = 0; */
        }

        /// <summary>
        /// Transfer part of the contents of the buffer to the display
        /// bounded by left, top, right and bottom
        /// Only supported in 16Bpp565 mode
        /// </summary>
        public override void Show(int left, int top, int right, int bottom)
        {
            if(colorMode != DisplayColorMode.Format16bppRgb565)
            {   //only supported in 565 mode 
                Show();
            }

            if(right < left || bottom < top)
            {   //could throw an exception
                return;
            }

            SetAddressWindow(left, top, right, bottom);

            var len = (right - left + 1) * sizeof(ushort);

            dataCommandPort.State = Data;

            int sourceIndex;
            for (int y = top; y <= bottom; y++)
            {
                sourceIndex = ((y * width) + left) * sizeof(ushort);

                spi.Exchange(chipSelectPort, writeBuffer.Span.Slice(sourceIndex, len), readBuffer.Span.Slice(sourceIndex, len));
            }
        }

        //probably move this to color struct
        private Color GetColorFromUShort(ushort color)
        {
            double r, g, b;
            if (colorMode == DisplayColorMode.Format16bppRgb565)
            {
                r = (color >> 11) / 31.0;
                g = ((color >> 5) & 0x3F) / 63.0;
                b = (color & 0x1F) / 31.0;

            }
            else
            {
                r = (color >> 8) / 15.0;
                g = ((color >> 4) & 0x0F) / 15.0;
                b = (color & 0x0F) / 15.0;
            }
            return new Color(r, g, b);
        }

        private ushort GetUShortFromColor(Color color)
        {
            if (colorMode == DisplayColorMode.Format16bppRgb565)
                return color.Color16bppRgb565;
            else //asume 12BppRgb444
                return color.Color12bppRgb444;
        }

        protected void Write(byte value)
        {
            spiDisplay.Write(value);
        }

        protected void Write(byte[] data)
        {
            spiDisplay.Write(data);
        }

        protected void DelayMs(int millseconds)
        {
            Thread.Sleep(millseconds);
        }

        protected void SendCommand(Register command)
        {
            SendCommand((byte)command);
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
            spiDisplay.Write(data);
        }

        /// <summary>
        /// Directly sets the display to a 16bpp color value
        /// </summary>
        /// <param name="color">16bpp color value (565)</param>
        public void ClearScreen(ushort color = 0)
        {
            // split the color in to two byte values
            var high = (byte)(color >> 8);
            var low = (byte)color;

            int index = 0;
            while (index < writeBuffer.Length)
            {
                writeBuffer.Span[index] = high;
                writeBuffer.Span[index + 1] = low;
                index += 2;
            }
        }

        public void Dispose()
        {
            spi = null;
            dataCommandPort = null;
            resetPort = null;
        }
    }
}