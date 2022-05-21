using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays.TftSpi
{
    public abstract partial class TftSpiBase : IGraphicsDriver
    {
        //these displays typically support 16 & 18 bit, some also include 8, 9, 12 and/or 24 bit color 

        public ColorType ColorMode => colorMode;
        protected ColorType colorMode;

        public abstract ColorType DefautColorMode { get; }
        public int Width => pixelBuffer.Width;
        public int Height => pixelBuffer.Height;
        public bool IgnoreOutOfBoundsPixels { get; set; }


        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalOutputPort chipSelectPort;
        protected ISpiPeripheral spiDisplay;

        protected IPixelBuffer pixelBuffer;
        protected Memory<byte> readBuffer;

       // protected int xMin, xMax, yMin, yMax;

        protected const bool Data = true;
        protected const bool Command = false;

        protected abstract void Initialize();

        internal TftSpiBase() { }


        public TftSpiBase(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width, int height, ColorType mode = ColorType.Format16bppRgb565)
        {
            dataCommandPort = device.CreateDigitalOutputPort(dcPin, false);
            if (resetPin != null) { resetPort = device.CreateDigitalOutputPort(resetPin, true); }
            if (chipSelectPin != null) { chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin, false); }

            spiDisplay = new SpiPeripheral(spiBus, chipSelectPort);

            CreateBuffer(mode, width, height);
        }


        /// <summary>
        ///     Draw the display buffer to screen
        /// </summary>
        public void Show()
        {
            SetAddressWindow(0, 0, Width - 1, Height);

            dataCommandPort.State = Data;

            //spiDisplay.Write(imageBuffer.Buffer);
            spiDisplay.Bus.Exchange(chipSelectPort, pixelBuffer.Buffer, readBuffer.Span);
        }

        /// <summary>
        /// Transfer part of the contents of the buffer to the display
        /// bounded by left, top, right and bottom
        /// Only supported in 16Bpp565 mode
        /// </summary>
        public void Show(int left, int top, int right, int bottom)
        {
            if (colorMode != ColorType.Format16bppRgb565)
            {   //only supported in 565 mode 
                Show();
            }

            if (right < left || bottom < top)
            {   //could throw an exception
                return;
            }

            SetAddressWindow(left, top, right, bottom);

            var len = (right - left + 1) * sizeof(ushort);

            dataCommandPort.State = Data;

            int sourceIndex;
            for (int y = top; y <= bottom; y++)
            {
                sourceIndex = ((y * Width) + left) * sizeof(ushort);

                //  spiDisplay.Write(imageBuffer.Buffer[sourceIndex..(sourceIndex + len)]);
                spiDisplay.Bus.Exchange(
                    chipSelectPort,
                    pixelBuffer.Buffer[sourceIndex..(sourceIndex + len)],
                    readBuffer.Span[0..len]);
            }
        }

        /// <summary>
        ///     Clear the display.
        /// </summary>
        /// <param name="updateDisplay">Update the dipslay once the buffer has been cleared when true.</param>
        public void Clear(bool updateDisplay = false)
        {
            pixelBuffer.Clear();

            if (updateDisplay) { Show(); }
        }

        public void Fill(Color color, bool updateDisplay = false)
        {
            pixelBuffer.Fill(color);
            if (updateDisplay)
            {
                Show();
            }
        }

        public void Fill(int x, int y, int width, int height, Color color)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0) x = 0;
                if (y < 0) y = 0;
                if (x > Width - 1) x = Width - 1;
                if (y > Height - 1) y = Height - 1;
            }

            pixelBuffer.Fill(color, x, y, width, height);
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="color">Color of pixel.</param>
        public void DrawPixel(int x, int y, Color color)
        {
            SafeSetPixel(x, y, color);
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="colored">Turn the pixel on (true) or off (false).</param>
        public void DrawPixel(int x, int y, bool colored)
        {
            //this works for now but it's a bit of a hack for 444
            SafeSetPixel(x, y, colored ? Color.White : Color.Black);
        }

        /// <summary>
        ///     Invert the color of a single pixel as represented in the display buffer
        /// </summary>
        /// <param name="x">x location</param>
        /// <param name="y">y location</param>
        public void InvertPixel(int x, int y)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
            }

            pixelBuffer.InvertPixel(x, y);
        }

        public void DrawBuffer(int x, int y, IPixelBuffer buffer)
        {
            pixelBuffer.WriteBuffer(x, y, buffer);
        }



        /// <summary>
        /// Wraps the IPixelBuffer.SetPixel method but does a 
        /// bounds check first
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        private void SafeSetPixel(int x, int y, Color color)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                { return; }
            }
            pixelBuffer.SetPixel(x, y, color);
        }
        public virtual bool IsColorModeSupported(ColorType mode)
        {
            if (mode == ColorType.Format12bppRgb444 ||
                mode == ColorType.Format16bppRgb565)
            {
                return true;
            }
            return false;
        }

        protected void CreateBuffer(ColorType mode, int width, int height)
        {
            switch (mode)
            {
                case ColorType.Format16bppRgb565:
                    pixelBuffer = new BufferRgb565(width, height);
                    break;
                case ColorType.Format12bppRgb444:
                    pixelBuffer = new BufferRgb444(width, height);
                    break;
                default:
                    throw new ArgumentException($"Mode {mode} not supported");

            }

            readBuffer = new byte[pixelBuffer.ByteCount];
            colorMode = mode;
        }

        protected abstract void SetAddressWindow(int x0, int y0, int x1, int y1);

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
    }
}