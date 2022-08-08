using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays.TftSpi
{
    public abstract partial class TftSpiBase : IGraphicsDisplay
    {
        //these displays typically support 16 & 18 bit, some also include 8, 9, 12 and/or 24 bit color 

        public ColorType ColorMode => imageBuffer.ColorMode;

        public abstract ColorType DefautColorMode { get; }
        public int Width => imageBuffer.Width;
        public int Height => imageBuffer.Height;

        public IPixelBuffer PixelBuffer => imageBuffer;

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalOutputPort chipSelectPort;
        protected ISpiPeripheral spiDisplay;

        protected IPixelBuffer imageBuffer;
        protected Memory<byte> readBuffer;

        protected const bool Data = true;
        protected const bool Command = false;

        protected abstract void Initialize();

        internal TftSpiBase()
        { }

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
        /// Is the color mode supported on this display
        /// </summary>
        /// <param name="mode">The color mode</param>
        /// <returns>true if supported</returns>
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
            if (IsColorModeSupported(mode) == false)
            {
                throw new ArgumentException($"Mode {mode} not supported");
            }

            if (mode == ColorType.Format24bppRgb888)
            {
                imageBuffer = new BufferRgb888(width, height);
            }

            else if (mode == ColorType.Format16bppRgb565)
            {
                imageBuffer = new BufferRgb565(width, height);
            }
            else //Rgb444
            {
                imageBuffer = new BufferRgb444(width, height);
            }
            readBuffer = new byte[imageBuffer.ByteCount];
        }

        protected abstract void SetAddressWindow(int x0, int y0, int x1, int y1);

        /// <summary>
        /// Clear the display.
        /// </summary>
        /// <param name="updateDisplay">Update the dipslay once the buffer has been cleared when true.</param>
        public void Clear(bool updateDisplay = false)
        {
            imageBuffer.Clear();

            if (updateDisplay) { Show(); }
        }

        /// <summary>
        /// Fill the display buffer with a color
        /// </summary>
        /// <param name="color">The fill color</param>
        /// <param name="updateDisplay">If true, update the display after filling the buffer</param>
        public void Fill(Color color, bool updateDisplay = false)
        {
            Clear(color);

            if (updateDisplay)
            {
                Show();
            }
        }

        public void WriteBuffer(int x, int y, IPixelBuffer buffer)
        {
            imageBuffer.WriteBuffer(x, y, buffer);
        }
   
        /// <summary>
        /// Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="colored">Turn the pixel on (true) or off (false).</param>
        public void DrawPixel(int x, int y, bool colored)
        {
            DrawPixel(x, y, colored ? Color.White : Color.Black);
        }

        /// <summary>
        /// Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="color">Color of pixel.</param>
        public void DrawPixel(int x, int y, Color color)
        {
            imageBuffer.SetPixel(x, y, color);
        }

        /// <summary>
        /// Draw a single pixel 
        /// </summary>
        /// <param name="x">x location</param>
        /// <param name="y">y location</param>
        /// <param name="r">8 bit red value</param>
        /// <param name="g">8 bit green value</param>
        /// <param name="b">8 bit blue value</param>
        public void DrawPixel(int x, int y, byte r, byte g, byte b)
        {
            DrawPixel(x, y, new Color(r, g, b));
        }

        /// <summary>
        /// Invert the color of a single pixel as represented in the display buffer
        /// </summary>
        /// <param name="x">x location</param>
        /// <param name="y">y location</param>
        public void InvertPixel(int x, int y)
        {
            PixelBuffer.InvertPixel(x, y);
        }

        public void Fill(int x, int y, int width, int height, Color color)
        {
            imageBuffer.Fill(x, y, width, height, color);
        }

        /// <summary>
        /// Draw the display buffer to screen
        /// </summary>
        public void Show()
        {
            SetAddressWindow(0, 0, Width - 1, Height);

            dataCommandPort.State = Data;

            spiDisplay.Bus.Exchange(chipSelectPort, imageBuffer.Buffer, readBuffer.Span);
        }

        /// <summary>
        /// Transfer part of the contents of the buffer to the display
        /// bounded by left, top, right and bottom
        /// Only supported in 16Bpp565 mode
        /// </summary>
        public void Show(int left, int top, int right, int bottom)
        {
            if(PixelBuffer.ColorMode != ColorType.Format16bppRgb565)
            {   //only supported in 565 mode 
                Show();
                return;
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
                sourceIndex = ((y * Width) + left) * sizeof(ushort);

                spiDisplay.Bus.Exchange(
                    chipSelectPort,
                    imageBuffer.Buffer[sourceIndex..(sourceIndex + len)],
                    readBuffer.Span[0..len]);
            }
        }
        /// <summary>
        /// Write a byte to the display
        /// </summary>
        /// <param name="value">The byte to send</param>
        protected void Write(byte value)
        {
            spiDisplay.Write(value);
        }

        /// <summary>
        /// Write a buffer to the display
        /// </summary>
        /// <param name="data">The data to send</param>
        protected void Write(byte[] data)
        {
            spiDisplay.Write(data);
        }

        /// <summary>
        /// Delay 
        /// </summary>
        /// <param name="millseconds">Milliseconds to delay</param>
        protected void DelayMs(int millseconds)
        {
            Thread.Sleep(millseconds);
        }

        /// <summary>
        /// Send a command to the display
        /// </summary>
        /// <param name="command">The command to send</param>
        protected void SendCommand(Register command)
        {
            SendCommand((byte)command);
        }

        /// <summary>
        /// Send a command to the display
        /// </summary>
        /// <param name="command">The command to send as a byte</param>
        protected void SendCommand(byte command)
        {
            dataCommandPort.State = Command;
            Write(command);
        }

        /// <summary>
        /// Send a single byte to the display (convenience method)
        /// </summary>
        /// <param name="data">The data to send </param>
        protected void SendData(int data)
        {
            SendData((byte)data);
        }

        /// <summary>
        /// Send a single byte to the display
        /// </summary>
        /// <param name="data">The byte to send</param>
        protected void SendData(byte data)
        {
            dataCommandPort.State = Data;
            Write(data);
        }

        /// <summary>
        /// Send a byte array of data to the display
        /// </summary>
        /// <param name="data">The data</param>
        protected void SendData(byte[] data)
        {
            dataCommandPort.State = Data;
            spiDisplay.Write(data);
        }

        /// <summary>
        /// Clear the display buffer to a color
        /// </summary>
        /// <param name="color">The clear color</param>
        public void Clear(Color color)
        {
            imageBuffer.Fill(color);
        }
    }
}