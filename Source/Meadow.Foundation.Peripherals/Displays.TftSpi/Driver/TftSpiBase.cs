using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Units;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Base class for TFT SPI displays
    /// These displays typically support 16 and 18 bit, some also include 8, 9, 12 and/or 24 bit color 
    /// </summary>
    public abstract partial class TftSpiBase : IPixelDisplay, ISpiPeripheral, IDisposable
    {
        /// <summary>
        /// Temporary buffer that can be used to batch set address window buffer commands
        /// </summary>
        protected byte[] SetAddressBuffer { get; } = new byte[4];

        /// <inheritdoc/>
        public ColorMode ColorMode => imageBuffer.ColorMode;

        /// <inheritdoc/>
        public abstract ColorMode SupportedColorModes { get; }

        /// <summary>
        /// The current rotation of the display
        /// </summary>
        public RotationType Rotation { get; protected set; } = RotationType.Normal;

        /// <summary>
        /// The display default color mode
        /// </summary>
        public abstract ColorMode DefaultColorMode { get; }

        /// <inheritdoc/>
        public int Width => imageBuffer.Width;

        /// <inheritdoc/>
        public int Height => imageBuffer.Height;

        /// <inheritdoc/>
        public IPixelBuffer PixelBuffer => imageBuffer;

        /// <summary>
        /// The default SPI bus speed for the device
        /// </summary>
        public virtual Frequency DefaultSpiBusSpeed => new Frequency(12000, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// The SPI bus speed for the device
        /// </summary>
        public Frequency SpiBusSpeed
        {
            get => spiDisplay.BusSpeed;
            set => spiDisplay.BusSpeed = value;
        }

        /// <summary>
        /// The default SPI bus mode for the device
        /// </summary>
        public virtual SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

        /// <summary>
        /// The SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode SpiBusMode
        {
            get => spiDisplay.BusMode;
            set => spiDisplay.BusMode = value;
        }

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPorts = false;

        /// <summary>
        /// The data command port
        /// </summary>
        protected IDigitalOutputPort dataCommandPort;

        /// <summary>
        /// The reset port
        /// </summary>
        protected IDigitalOutputPort? resetPort;

        /// <summary>
        /// The chip select port
        /// </summary>
        protected IDigitalOutputPort? chipSelectPort;

        /// <summary>
        /// The spi peripheral for the display
        /// </summary>
        protected ISpiCommunications spiDisplay;

        /// <summary>
        /// The off-screen image buffer
        /// </summary>
        protected IPixelBuffer imageBuffer = default!;

        /// <summary>
        /// The read buffer
        /// </summary>
        protected Memory<byte> readBuffer;

        /// <summary>
        /// Data convenience bool
        /// </summary>
        protected const bool Data = true;

        /// <summary>
        /// Command convenience bool
        /// </summary>
        protected const bool Command = false;

        /// <summary>
        /// Initialize the display
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// The display's native height without rotation
        /// </summary>
        protected int nativeHeight;

        /// <summary>
        /// The display's native width without rotation
        /// </summary>
        protected int nativeWidth;

        /// <summary>
        /// Previous x0 value passed to SetAddressWindow
        /// Used for optimization to avoid unnecessary SPI commands
        /// </summary>
        protected int setAddressLastX0 = -1;

        /// <summary>
        /// Previous x1 value passed to SetAddressWindow
        /// Used for optimization to avoid unnecessary SPI commands
        /// </summary>
        protected int setAddressLastX1 = -1;

        /// <summary>
        /// Previous y0 value passed to SetAddressWindow
        /// Used for optimization to avoid unnecessary SPI commands
        /// </summary>
        protected int setAddressLastY0 = -1;

        /// <summary>
        /// Previous y1 value passed to SetAddressWindow
        /// Used for optimization to avoid unnecessary SPI commands
        /// </summary>
        protected int setAddressLastY1 = -1;

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
        public TftSpiBase(ISpiBus spiBus, IPin? chipSelectPin, IPin dcPin, IPin? resetPin,
            int width, int height, ColorMode colorMode = ColorMode.Format16bppRgb565)
            : this(
                    spiBus,
                    chipSelectPin?.CreateDigitalOutputPort() ?? null,
                    dcPin.CreateDigitalOutputPort(),
                    resetPin?.CreateDigitalOutputPort(),
                    width, height, colorMode
                  )
        {
            createdPorts = true;
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
            IDigitalOutputPort? chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort? resetPort,
            int width, int height,
            ColorMode colorMode = ColorMode.Format16bppRgb565)
        {
            this.dataCommandPort = dataCommandPort;
            this.chipSelectPort = chipSelectPort;
            this.resetPort = resetPort;

            spiDisplay = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

            CreateBuffer(colorMode, nativeWidth = width, nativeHeight = height);
        }

        /// <summary>
        /// Set color mode for the display
        /// </summary>
        /// <param name="colorMode"></param>
        /// <exception cref="ArgumentException">throw if the color mode isn't supported by the display</exception>
        public virtual void SetColorMode(ColorMode colorMode)
        {
            if (IsColorTypeSupported(colorMode) == false)
            {
                throw new ArgumentException($"color mode {colorMode} not supported");
            }

            if (imageBuffer.ColorMode != colorMode)
            {
                CreateBuffer(colorMode, Width, Height);
                Initialize();
            }
        }

        /// <summary>
        /// Is the color mode supported on this display
        /// </summary>
        /// <param name="colorType">The color mode</param>
        /// <returns>true if supported</returns>
        public virtual bool IsColorTypeSupported(ColorMode colorType)
        {
            return (SupportedColorModes | colorType) != 0;
        }

        /// <summary>
        /// Create an off-screen buffer for the display
        /// </summary>
        /// <param name="colorMode">The color mode</param>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        /// <exception cref="ArgumentException">Throws an exception if the color mode isn't supported</exception>
        protected void CreateBuffer(ColorMode colorMode, int width, int height)
        {
            if (IsColorTypeSupported(colorMode) == false)
            {
                throw new ArgumentException($"color mode {colorMode} not supported");
            }

            if (colorMode == ColorMode.Format24bppRgb888)
            {
                imageBuffer = new BufferRgb888(width, height);
            }

            else if (colorMode == ColorMode.Format16bppRgb565)
            {
                imageBuffer = new BufferRgb565(width, height);
            }
            else
            {
                imageBuffer = new BufferRgb444(width, height);
            }
            readBuffer = new byte[imageBuffer.ByteCount];
        }

        /// <summary>
        /// Set address window for display updates
        /// </summary>
        /// <param name="x0">X start in pixels</param>
        /// <param name="y0">Y start in pixels</param>
        /// <param name="x1">X end in pixels</param>
        /// <param name="y1">Y end in pixels</param>
        protected virtual void SetAddressWindow(int x0, int y0, int x1, int y1)
        {
            if (x0 != setAddressLastX0 || x1 != setAddressLastX1 || y0 != setAddressLastY0 || y1 != setAddressLastY1)
            {
                setAddressLastX0 = x0;
                setAddressLastX1 = x1;
                setAddressLastY0 = y0;
                setAddressLastY1 = y1;

                SendCommand(LcdCommand.CASET);  // column addr set
                dataCommandPort.State = Data;
                SetAddressBuffer[0] = (byte)(x0 >> 8);
                SetAddressBuffer[1] = (byte)(x0 & 0xff); // XSTART
                SetAddressBuffer[2] = (byte)(x1 >> 8);
                SetAddressBuffer[3] = (byte)(x1 & 0xff); // XEND
                Write(SetAddressBuffer);

                SendCommand(LcdCommand.RASET);  // row addr set
                dataCommandPort.State = Data;
                SetAddressBuffer[0] = (byte)(y0 >> 8);
                SetAddressBuffer[1] = (byte)(y0 & 0xff); // XEND
                SetAddressBuffer[2] = (byte)(y1 >> 8);
                SetAddressBuffer[3] = (byte)(y1 & 0xff); // YEND
                Write(SetAddressBuffer);

                SendCommand(LcdCommand.RAMWR);  // write to RAM
            }
        }

        /// <summary>
        /// Clear the display.
        /// </summary>
        /// <param name="updateDisplay">Update the display once the buffer has been cleared when true.</param>
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
        /// Write a buffer to the display off-screen buffer
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
            SetAddressWindow(0, 0, Width - 1, Height - 1);

            dataCommandPort.State = Data;

            spiDisplay.Bus.Exchange(chipSelectPort, imageBuffer.Buffer, readBuffer.Span);
        }

        /// <summary>
        /// Transfer part of the contents of the buffer to the display
        /// bounded by left, top, right and bottom
        /// </summary>
        public void Show(int left, int top, int right, int bottom)
        {
            if (PixelBuffer.ColorMode != ColorMode.Format12bppRgb444 &&
                PixelBuffer.ColorMode != ColorMode.Format16bppRgb565 &&
                PixelBuffer.ColorMode != ColorMode.Format24bppRgb888)
            {
                //should cover all of these displays but just in case
                Show();
                return;
            }

            if (right < left || bottom < top)
            {   //could throw an exception
                return;
            }

            if (PixelBuffer.ColorMode == ColorMode.Format12bppRgb444)
            {
                if (left % 2 != 0)
                {
                    left--;
                }
                if (right % 2 != 0)
                {
                    right++;
                }
            }

            SetAddressWindow(left, top, right - 1, bottom - 1);

            var len = (right - left) * PixelBuffer.BitDepth / 8;

            dataCommandPort.State = Data;

            int sourceIndex;
            for (int y = top; y < bottom; y++)
            {
                sourceIndex = ((y * Width) + left) * PixelBuffer.BitDepth / 8;

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

        /// <summary>
        /// Update the display buffer if the dimensions change on rotation
        /// </summary>
        protected void UpdateBuffer()
        {
            var newWidth = Rotation switch
            {
                RotationType._90Degrees => nativeHeight,
                RotationType._270Degrees => nativeHeight,
                _ => nativeWidth
            };

            var newHeight = Rotation switch
            {
                RotationType._90Degrees => nativeWidth,
                RotationType._270Degrees => nativeWidth,
                _ => nativeHeight
            };

            if (newWidth != Width)
            {
                CreateBuffer(ColorMode, newWidth, newHeight);
            }
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPorts)
                {
                    chipSelectPort?.Dispose();
                    dataCommandPort?.Dispose();
                    resetPort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}