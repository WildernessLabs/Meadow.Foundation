using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Units;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Provide an interface to the Sh110x family of displays
    /// </summary>
    public abstract partial class Sh110x : IPixelDisplay, ISpiPeripheral, II2cPeripheral, IDisposable
    {
        /// <inheritdoc/>
        public ColorMode ColorMode => ColorMode.Format1bpp;

        /// <inheritdoc/>
        public ColorMode SupportedColorModes => ColorMode.Format1bpp;

        /// <inheritdoc/>
        public int Width { get; protected set; }

        /// <inheritdoc/>
        public int Height { get; protected set; }

        /// <summary>
        /// The default SPI bus speed for the device
        /// </summary>
        public Frequency DefaultSpiBusSpeed => new Frequency(4, Frequency.UnitType.Megahertz);

        /// <summary>
        /// The SPI bus speed for the device
        /// </summary>
        public Frequency SpiBusSpeed
        {
            get => spiComms!.BusSpeed;
            set => spiComms!.BusSpeed = value;
        }

        /// <summary>
        /// The default SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

        /// <summary>
        /// The SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode SpiBusMode
        {
            get => spiComms!.BusMode;
            set => spiComms!.BusMode = value;
        }

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// The connection type (I2C or SPI)
        /// </summary>
        protected ConnectionType connectionType;

        /// <summary>
        /// The buffer the holds the pixel data for the display
        /// </summary>
        public IPixelBuffer PixelBuffer => imageBuffer;

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        private readonly bool createdPorts = false;

        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications? i2cComms;

        /// <summary>
        /// SPI Communication bus used to communicate with the peripheral
        /// </summary>
        protected ISpiCommunications? spiComms;

        private readonly IDigitalOutputPort? chipSelectPort;
        private readonly IDigitalOutputPort? dataCommandPort;
        private readonly IDigitalOutputPort? resetPort;

        private readonly int pageWidth;
        private readonly int totalPages;
        private readonly byte startColumnOffset;
        private byte startColumnOffsetLow => (byte)(startColumnOffset & 0x0F);
        private byte startColumnOffsetHigh => (byte)((startColumnOffset & 0xF0) >> 4);

        private const bool Data = true;
        private const bool Command = false;

        private readonly Buffer1bpp imageBuffer;
        private readonly byte[] pageBuffer;

        /// <summary>
        /// Display command buffer
        /// </summary>
        protected Memory<byte> commandBuffer;

        /// <summary>
        /// Create a new Sh110x object
        /// </summary>
        /// <param name="i2cBus">I2C bus connected to display</param>
        /// <param name="address">I2C address</param>
        /// <param name="width">Display width in pixels</param>
        /// <param name="height">Display height in pixels</param>
        /// <param name="firstColumn">The first visible column on the display (if display is cropped)</param>
        public Sh110x(II2cBus i2cBus, byte address, int width, int height, int firstColumn = 0)
        {
            connectionType = ConnectionType.I2C;
            i2cComms = new I2cCommunications(i2cBus, address);

            Width = width;
            Height = height;

            commandBuffer = new byte[2];

            imageBuffer = new Buffer1bpp(Width, Height);
            startColumnOffset = (byte)firstColumn;
            pageWidth = Width;
            totalPages = height >> 3;
            pageBuffer = new byte[pageWidth + 1];

            Initialize();
        }

        /// <summary>
        /// Create a new Sh110x object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Display width in pixels</param>
        /// <param name="height">Display height in pixels</param>
        /// <param name="firstColumn">The first visible column on the display (if display is cropped)</param>
        public Sh110x(ISpiBus spiBus, IPin? chipSelectPin, IPin dcPin, IPin resetPin, int width, int height, int firstColumn = 0) :
            this(spiBus, chipSelectPin?.CreateDigitalOutputPort(), dcPin.CreateDigitalOutputPort(),
                resetPin.CreateDigitalOutputPort(), width, height, firstColumn)
        {
            createdPorts = true;
        }

        /// <summary>
        /// Create a new Sh110x display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Display width in pixels</param>
        /// <param name="height">Display height in pixels</param>
        /// <param name="firstColumn">The first visible column on the display (if display is cropped)</param>
        public Sh110x(ISpiBus spiBus, IDigitalOutputPort? chipSelectPort, IDigitalOutputPort dataCommandPort, IDigitalOutputPort resetPort,
            int width, int height, int firstColumn = 0)
        {
            connectionType = ConnectionType.SPI;

            this.chipSelectPort = chipSelectPort;
            this.dataCommandPort = dataCommandPort;
            this.resetPort = resetPort;

            spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

            Width = width;
            Height = height;

            imageBuffer = new Buffer1bpp(Width, Height);
            startColumnOffset = (byte)firstColumn;
            pageWidth = Width;
            pageBuffer = new byte[pageWidth];

            Initialize();
        }

        /// <summary>
        /// This varies between manufacturers 
        /// If the display is misaligned horizontally, try offset values like 0x00, 0x20, 0x40, etc.
        /// If the display is misaligned vertically, try offset values like 0x00, 0x20, 0x40, etc.
        /// </summary>
        /// <param name="startLine">Line number in display RAM to display at the top of the screen</param>
        /// <param name="offset">Column number in display RAM to offset the screen</param>
        public abstract void SetDisplayOffsets(byte startLine = 0x00, byte offset = 0x00);

        /// <summary>
        /// This varies between manufacturers 
        /// If the display is misaligned horizontally, try offset values like 0x00, 0x20, 0x40, etc.
        /// </summary>
        /// <param name="offset">Column number in display RAM to offset the screen</param>
        public void SetDisplayOffset(byte offset = 0x00)
        {
            SendCommands(new[] { (byte)DisplayCommand.SetDisplayOffset, offset });
        }

        /// <summary>
        /// Invert the entire display (true) or return to normal mode (false)
        /// </summary>
        public void InvertDisplay(bool invert)
        {
            SendCommand(invert ? DisplayCommand.DisplayVideoReverse : DisplayCommand.DisplayVideoNormal);
        }

        /// <summary>
        /// Set display into power saving mode
        /// </summary>
        public void PowerSaveMode()
        {
            SendCommand(DisplayCommand.DisplayOff);
            SendCommand(DisplayCommand.DisplayAllPixelsOn);
        }

        /// <summary>
        /// Reset for SPI displays
        /// </summary>
        protected void Reset()
        {
            if (resetPort != null)
            {
                resetPort.State = true;
                Thread.Sleep(10);
                resetPort.State = false;
                Thread.Sleep(10);
                resetPort.State = true;
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Initialize the display
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// Set the display contrast 
        /// </summary>
        /// <param name="contrast">The contrast value (0-255)</param>
        public void SetContrast(byte contrast)
        {
            SendCommand(DisplayCommand.SetContrast);
            SendCommand(contrast);
        }

        /// <summary>
        /// Send a command to the display
        /// </summary>
        /// <param name="command">Command byte to send to the display</param>
        internal void SendCommand(byte command)
        {
            if (connectionType == ConnectionType.SPI)
            {
                dataCommandPort!.State = Command;
                spiComms?.Write(command);
            }
            else
            {
                commandBuffer.Span[0] = 0x00;
                commandBuffer.Span[1] = command;
                i2cComms?.Write(commandBuffer.Span);
            }
        }

        /// <summary>
        /// Send a command to the display
        /// </summary>
        /// <param name="command">Command byte to send to the display</param>
        internal void SendCommand(DisplayCommand command)
        {
            SendCommand((byte)command);
        }

        internal void SendCommands(DisplayCommand[] commands)
        {
            var bytes = new byte[commands.Length];
            commands.CopyTo(bytes, 0);
            SendCommands(bytes);
        }

        /// <summary>
        /// Send a sequence of commands to the display
        /// </summary>
        /// <param name="commands">List of bytes to send</param>
        internal void SendCommands(byte[] commands)
        {
            if (connectionType == ConnectionType.SPI)
            {
                dataCommandPort!.State = Command;
                spiComms?.Write(commands);
            }
            else
            {
                Span<byte> data = new byte[commands.Length + 1];
                data[0] = 0x00;
                commands.CopyTo(data.Slice(1, commands.Length));
                i2cComms?.Write(data);
            }
        }

        /// <summary>
        /// Send the internal pixel buffer to display
        /// </summary>
        public void Show()
        {
            for (byte page = 0; page < totalPages; page++)
            {
                SendCommand((byte)((byte)DisplayCommand.PageAddress + page));
                SendCommand((byte)((byte)DisplayCommand.ColumnAddressLow + startColumnOffsetLow));
                SendCommand((byte)((byte)DisplayCommand.ColumnAddressHigh + startColumnOffsetHigh));

                if (connectionType == ConnectionType.SPI)
                {
                    dataCommandPort!.State = Data;

                    Array.Copy(imageBuffer.Buffer, Width * page, pageBuffer, 0, pageWidth);
                    spiComms?.Write(pageBuffer);
                }
                else // I2C
                {
                    pageBuffer[0] = 0x40;

                    Array.Copy(imageBuffer.Buffer, Width * page, pageBuffer, 1, pageWidth);
                    i2cComms?.Write(pageBuffer);
                }
            }
        }

        /// <summary>
        /// Update a region of the display from the offscreen buffer.
        /// </summary>
        /// <param name="left">Left bounds in pixels (Currently ignored)</param>
        /// <param name="top">Top bounds in pixels</param>
        /// <param name="right">Right bounds in pixels (Currently ignored)</param>
        /// <param name="bottom">Bottom bounds in pixels</param>
        /// <remarks>This method currently increases the update area to cover the full area of all affected Width * 8 'pages'</remarks>
        public void Show(int left, int top, int right, int bottom)
        {
            const int pageHeight = 8;

            // currently only updates in pages (area of Width x 8 pixels)
            // iterate over all pages and check if they're in range
            for (byte page = 0; page < totalPages; page++)
            {
                if (top > pageHeight * page || bottom < (page + 1) * pageHeight)
                {
                    continue;
                }

                SendCommand((byte)(DisplayCommand.PageAddress + page));
                SendCommand((byte)((byte)DisplayCommand.ColumnAddressLow + startColumnOffsetLow));
                SendCommand((byte)((byte)DisplayCommand.ColumnAddressHigh + startColumnOffsetHigh));
                if (connectionType == ConnectionType.SPI)
                {
                    dataCommandPort!.State = Data;

                    Array.Copy(imageBuffer.Buffer, Width * page, pageBuffer, 0, pageWidth);
                    spiComms?.Write(pageBuffer);
                }
                else // I2C
                {
                    pageBuffer[0] = 0x40;

                    Array.Copy(imageBuffer.Buffer, Width * page, pageBuffer, 1, pageWidth);
                    i2cComms?.Write(pageBuffer);
                }
            }
        }

        /// <summary>
        /// Clear the display buffer
        /// </summary>
        /// <param name="updateDisplay">Immediately update the display when true</param>
        public void Clear(bool updateDisplay = false)
        {
            imageBuffer.Clear();

            if (updateDisplay) { Show(); }
        }

        /// <summary>
        /// Draw pixel at a location
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset</param>
        /// <param name="y">Ordinate of the pixel to the set / reset</param>
        /// <param name="color">Any color = turn on pixel, black = turn off pixel</param>
        public void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color1bpp);
        }

        /// <summary>
        /// Draw pixel at a location
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset</param>
        /// <param name="y">Ordinate of the pixel to the set / reset</param>
        /// <param name="enabled">True = turn on pixel, false = turn off pixel</param>
        public void DrawPixel(int x, int y, bool enabled)
        {
            imageBuffer.SetPixel(x, y, enabled);
        }

        /// <summary>
        /// Invert a pixel at a location
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset</param>
        /// <param name="y">Ordinate of the pixel to the set / reset</param>
        public void InvertPixel(int x, int y)
        {
            imageBuffer.InvertPixel(x, y);
        }

        /// <summary>
        /// Start the display scrolling in the specified direction.
        /// </summary>
        /// <param name="direction">Direction that the display should scroll</param>
        public void StartScrolling(ScrollDirection direction)
        {
            StartScrolling(direction, 0x00, 0xff);
        }

        /// <summary>
        /// Start the display scrolling
        /// </summary>
        /// <remarks>
        /// In most cases setting startPage to 0x00 and endPage to 0xff will achieve an
        /// acceptable scrolling effect
        /// </remarks>
        /// <param name="direction">Direction that the display should scroll</param>
        /// <param name="startPage">Start page for the scroll</param>
        /// <param name="endPage">End page for the scroll</param>
        public void StartScrolling(ScrollDirection direction, byte startPage, byte endPage)
        {
            StopScrolling();
            byte[] commands;
            if ((direction == ScrollDirection.Left) || (direction == ScrollDirection.Right))
            {
                commands = new byte[] { 0x26, 0x00, startPage, 0x00, endPage, 0x00, 0xff, 0x2f };

                if (direction == ScrollDirection.Left)
                {
                    commands[0] = 0x27;
                }
            }
            else
            {
                byte scrollDirection;

                if (direction == ScrollDirection.LeftAndVertical)
                {
                    scrollDirection = 0x2a;
                }
                else
                {
                    scrollDirection = 0x29;
                }

                commands = new byte[] { 0xa3, 0x00, (byte)Height, scrollDirection, 0x00, startPage, 0x00, endPage, 0x01, 0x2f };
            }
            SendCommands(commands);
        }

        /// <summary>
        /// Turn off scrolling
        /// </summary>
        /// <remarks>
        /// Datasheet states that scrolling must be turned off before changing the
        /// scroll direction in order to prevent RAM corruption
        /// </remarks>
        public void StopScrolling()
        {
            SendCommand(0x2e);
        }

        /// <summary>
        /// Fill display buffer with a color
        /// </summary>
        /// <param name="clearColor">The fill color</param>
        /// <param name="updateDisplay">If true, update display</param>
        public void Fill(Color clearColor, bool updateDisplay = false)
        {
            imageBuffer.Clear(clearColor.Color1bpp);

            if (updateDisplay) Show();
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
        /// Write a buffer to the display offscreen buffer
        /// </summary>
        /// <param name="x">The x position in pixels to write the buffer</param>
        /// <param name="y">The y position in pixels to write the buffer</param>
        /// <param name="displayBuffer">The buffer to write</param>
        public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
        {
            imageBuffer.WriteBuffer(x, y, displayBuffer);
        }

        #region IDisposible
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
        #endregion
    }
}