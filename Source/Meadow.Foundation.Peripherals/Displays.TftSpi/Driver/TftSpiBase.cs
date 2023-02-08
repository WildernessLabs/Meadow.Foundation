using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    public abstract partial class TftSpiBase : IGraphicsDisplay
    {
        //these displays typically support 16 & 18 bit, some also include 8, 9, 12 and/or 24 bit color 

        /// <summary>
        /// The current display color mode
        /// </summary>
        public ColorMode ColorMode => imageBuffer.ColorMode;

        /// <summary>
        /// The color modes supported by the display
        /// </summary>
        public abstract ColorMode SupportedColorModes { get; }

        /// <summary>
        /// The current rotation of the display
        /// </summary>
        public RotationType Rotation { get; set; } = RotationType.Normal;

        /// <summary>
        /// The display default color mode
        /// </summary>
        public abstract ColorMode DefautColorMode { get; }

        /// <summary>
        /// Width of display in pixels
        /// </summary>
        public int Width => imageBuffer.Width;

        /// <summary>
        /// Height of display in pixels
        /// </summary>
        public int Height => imageBuffer.Height;

        /// <summary>
        /// The buffer used to store the pixel data for the display
        /// </summary>
        public IPixelBuffer PixelBuffer => imageBuffer;

        /// <summary>
        /// The data command port
        /// </summary>
        protected IDigitalOutputPort dataCommandPort;

        /// <summary>
        /// The reset port
        /// </summary>
        protected IDigitalOutputPort resetPort;

        /// <summary>
        /// The chip select port
        /// </summary>
        protected IDigitalOutputPort chipSelectPort;

        /// <summary>
        /// The spi peripheral for the display
        /// </summary>
        protected ISpiPeripheral spiDisplay;

        /// <summary>
        /// The offscreen image buffer
        /// </summary>
        protected IPixelBuffer imageBuffer;

        /// <summary>
        /// The read buffer
        /// </summary>
        protected Memory<byte> readBuffer;

        /// <summary>
        /// Data convience bool
        /// </summary>
        protected const bool Data = true;

        /// <summary>
        /// Command convenience bool
        /// </summary>
        protected const bool Command = false;

        /// <summary>
        /// Initalize the display
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// Represents an abstract TftSpiBase object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public TftSpiBase(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width, int height, ColorMode colorMode = ColorMode.Format16bppRgb565)
            : this(
                    spiBus,
                    chipSelectPin.CreateDigitalOutputPort(),
                    dcPin.CreateDigitalOutputPort(),
                    resetPin.CreateDigitalOutputPort(),
                    width, height, colorMode
                  )
        {
        }

        /// <summary>
        /// Represents an abstract TftSpiBase object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public TftSpiBase(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            int width, int height,
            ColorMode colorMode = ColorMode.Format16bppRgb565)
        {
            this.dataCommandPort = dataCommandPort;
            this.chipSelectPort = chipSelectPort;
            this.resetPort = resetPort;

            spiDisplay = new SpiPeripheral(spiBus, chipSelectPort);

            CreateBuffer(colorMode, width, height);
        }

        /// <summary>
        /// Is the color mode supported on this display
        /// </summary>
        /// <param name="colorType">The color mode</param>
        /// <returns>true if supported</returns>
        public virtual bool IsColorTypeSupported(ColorMode colorType)
        {
            return (SupportedColorModes | colorType) != 0;
            /*
            if (SupportedColors)


            if (mode == ColorType.Format12bppRgb444 ||
                mode == ColorType.Format16bppRgb565)
            {
                return true;
            }
            return false;*/
        }

        /// <summary>
        /// Create an offscreen buffer for the display
        /// </summary>
        /// <param name="mode">The color mode</param>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        /// <exception cref="ArgumentException">Throws an exception if the color mode isn't supported</exception>
        protected void CreateBuffer(ColorMode colorType, int width, int height)
        {
            if (IsColorTypeSupported(colorType) == false)
            {
                throw new ArgumentException($"color mode {colorType} not supported");
            }

            if (colorType == ColorMode.Format24bppRgb888)
            {
                imageBuffer = new BufferRgb888(width, height);
            }

            else if (colorType == ColorMode.Format16bppRgb565)
            {
                imageBuffer = new BufferRgb565(width, height);
            }
            else //Rgb444
            {
                imageBuffer = new BufferRgb444(width, height);
            }
            readBuffer = new byte[imageBuffer.ByteCount];
        }

        /// <summary>
        /// Set addrees window for display updates
        /// </summary>
        /// <param name="x0">X start in pixels</param>
        /// <param name="y0">Y start in pixels</param>
        /// <param name="x1">X end in pixels</param>
        /// <param name="y1">Y end in pixels</param>
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

        /// <summary>
        /// Write a buffer to the display offscreen buffer
        /// </summary>
        /// <param name="x">The x position in pixels to write the buffer</param>
        /// <param name="y">The y position in pixels to write the buffer</param>
        /// <param name="buffer">The buffer to write</param>
        public void WriteBuffer(int x, int y, IPixelBuffer buffer)
        {
            imageBuffer.WriteBuffer(x, y, buffer);
        }

        /// <summary>
        /// Draw pixel at a location
        /// Primarily used for monochrome displays, prefer overload that accepts a Color
        /// </summary>
        /// <param name="x">x location in pixels</param>
        /// <param name="y">y location in pixels</param>
        /// <param name="enabled">Turn the pixel on (true) or off (false).</param>
        public void DrawPixel(int x, int y, bool enabled)
        {
            DrawPixel(x, y, enabled ? Color.White : Color.Black);
        }

        /// <summary>
        /// Draw a single pixel 
        /// </summary>
        /// <param name="x">x location in pixels</param>
        /// <param name="y">y location in pixels</param>
        /// <param name="color">Color of pixel.</param>
        public void DrawPixel(int x, int y, Color color)
        {
            imageBuffer.SetPixel(x, y, color);
        }

        /// <summary>
        /// Draw a single pixel 
        /// </summary>
        /// <param name="x">x location in pixels</param>
        /// <param name="y">y location in pixels</param>
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
        /// <param name="x">x location in pixels</param>
        /// <param name="y">y location in pixels</param>
        public void InvertPixel(int x, int y)
        {
            PixelBuffer.InvertPixel(x, y);
        }

        /// <summary>
        /// Fill with a color
        /// </summary>
        /// <param name="x">X start position in pixels</param>
        /// <param name="y">Y start position in pixels</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <param name="color">The fill color</param>
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
            if (PixelBuffer.ColorMode != ColorMode.Format16bppRgb565)
            {   //only supported in 565 mode 
                Show();
                return;
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
        /// <param name="command">The command to send as an LcdCommand</param>
        protected void SendCommand(LcdCommand command)
        {
            dataCommandPort.State = Command;
            Write((byte)command);
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