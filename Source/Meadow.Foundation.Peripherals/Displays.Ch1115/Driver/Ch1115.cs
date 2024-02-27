using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Units;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Provide an interface to the Ch1115 family of displays
    /// </summary>
    public partial class Ch1115 : IPixelDisplay, ISpiPeripheral, IDisposable
    {
        /// <inheritdoc/>
        public ColorMode ColorMode => ColorMode.Format1bpp;

        /// <inheritdoc/>
        public ColorMode SupportedColorModes => ColorMode.Format1bpp;

        /// <inheritdoc/>
        public int Width => imageBuffer.Width;

        /// <inheritdoc/>
        public int Height => imageBuffer.Height;

        /// <inheritdoc/>
        public IPixelBuffer PixelBuffer => imageBuffer;

        /// <summary>
        /// The default SPI bus speed for the device
        /// </summary>
        public Frequency DefaultSpiBusSpeed => new(375, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// The SPI bus speed for the device
        /// </summary>
        public Frequency SpiBusSpeed
        {
            get => spiComms.BusSpeed;
            set => spiComms.BusSpeed = value;
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
            get => spiComms.BusMode;
            set => spiComms.BusMode = value;
        }

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// SPI Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly ISpiCommunications spiComms;

        readonly IDigitalOutputPort dataCommandPort;
        readonly IDigitalOutputPort resetPort;
        readonly IDigitalOutputPort? chipSelectPort;

        const bool Data = true;
        const bool Command = false;

        readonly Buffer1bpp imageBuffer;
        readonly byte[] pageBuffer;

        readonly bool createdPorts = false;

        /// <summary>
        /// Create a new Ch1115 object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Ch1115(ISpiBus spiBus, IPin? chipSelectPin, IPin dcPin, IPin resetPin,
            int width = 128, int height = 64) :
            this(spiBus, chipSelectPin?.CreateDigitalOutputPort() ?? null, dcPin.CreateDigitalOutputPort(),
                resetPin.CreateDigitalOutputPort(), width, height)
        {
            createdPorts = true;
        }

        /// <summary>
        /// Create a new Ch1115 display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Ch1115(ISpiBus spiBus,
            IDigitalOutputPort? chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            int width = 128, int height = 64)
        {
            this.dataCommandPort = dataCommandPort;
            this.resetPort = resetPort;
            this.chipSelectPort = chipSelectPort;

            spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

            imageBuffer = new Buffer1bpp(width, height);
            pageBuffer = new byte[PageSize];

            Initialize();
        }

        /// <summary>
        /// Invert the entire display (true) or return to normal mode (false).
        /// </summary>
        public void InvertDisplay(bool invert)
        {
            if (invert)
            {
                SendCommand(DisplayCommand.DisplayVideoReverse);
            }
            else
            {
                SendCommand(DisplayCommand.DisplayVideoNormal);
            }
        }

        /// <summary>
        /// Set display into power saving mode
        /// </summary>
        public void PowerSaveMode()
        {
            SendCommand(DisplayCommand.DisplayOff);
            SendCommand(DisplayCommand.AllPixelsOn);
        }

        void SendCommand(DisplayCommand command)
        {
            SendCommand((byte)command);
        }

        void Reset()
        {
            resetPort.State = true;
            Thread.Sleep(10);
            resetPort.State = false;
            Thread.Sleep(10);
            resetPort.State = true;
            Thread.Sleep(100);
        }

        private void Initialize()
        {
            Reset();

            SendCommand(DisplayCommand.DisplayOff);
            SendCommand(DisplayCommand.ColumnAddressLow);
            SendCommand(DisplayCommand.ColumnAddressHigh);
            SendCommand(DisplayCommand.PageAddress);
            SendCommand(DisplayCommand.DisplayStartLine);

            SetContrast(32);

            SendCommand(DisplayCommand.IRefRestigerSet);
            SendCommand(DisplayCommand.IRefRestigerAdjust);

            SendCommand(DisplayCommand.SegSetRemap);
            SendCommand(DisplayCommand.SegSetPads);
            SendCommand(DisplayCommand.AllPixelsOn);
            SendCommand(DisplayCommand.DisplayVideoNormal);

            SendCommand(DisplayCommand.MultiplexModeSet);
            SendCommand(DisplayCommand.MultiplexDataSet);

            SendCommand(DisplayCommand.CommonScanDir);
            SendCommand(DisplayCommand.OffsetModeSet);
            SendCommand(DisplayCommand.OffsetDataSet);

            SendCommand(DisplayCommand.OscFrequencyModeSet);
            SendCommand(DisplayCommand.OscFrequencyDataSet);

            SendCommand(DisplayCommand.PrechargeModeSet);
            SendCommand(DisplayCommand.PrechargeDataSet);

            SendCommand(DisplayCommand.ComLevelModeSet);
            SendCommand(DisplayCommand.ComLevelDataSet);

            SendCommand(DisplayCommand.SetPumpSet | DisplayCommand.SetPumpSet);

            SendCommand(DisplayCommand.DCModeSet);
            SendCommand(DisplayCommand.DCOnOffSet);

            SendCommand(DisplayCommand.DisplayOn);
            SendCommand(DisplayCommand.AllPixelsOff);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Set the display contrast 
        /// </summary>
        /// <param name="contrast">The contrast value (0-63)</param>
        public void SetContrast(byte contrast)
        {
            SendCommand(DisplayCommand.ContrastValue);
            SendCommand((byte)((int)DisplayCommand.ContrastValue | (contrast & 0x3f)));
        }

        /// <summary>
        /// Send a command to the display
        /// </summary>
        /// <param name="command">Command byte to send to the display</param>
        private void SendCommand(byte command)
        {
            dataCommandPort.State = Command;
            spiComms.Write(command);
        }

        /// <summary>
        /// Send a sequence of commands to the display
        /// </summary>
        /// <param name="commands">List of commands to send</param>
        private void SendCommands(byte[] commands)
        {
            var data = new byte[commands.Length + 1];
            data[0] = 0x00;
            Array.Copy(commands, 0, data, 1, commands.Length);

            dataCommandPort.State = Command;
            spiComms.Write(commands);
        }

        const int StartColumnOffset = 0;
        const int PageSize = 128;

        /// <summary>
        /// Send the internal pixel buffer to display
        /// </summary>
        public void Show()
        {
            for (int page = 0; page < 8; page++)
            {
                SendCommand((DisplayCommand.ColumnAddressLow) | (StartColumnOffset & 0x0F));
                SendCommand((int)DisplayCommand.ColumnAddressHigh);
                SendCommand((byte)((byte)DisplayCommand.PageAddress | page));

                dataCommandPort.State = Data;

                Array.Copy(imageBuffer.Buffer, Width * page, pageBuffer, 0, PageSize);
                spiComms.Write(pageBuffer);
            }
        }

        /// <summary>
        /// Update a region of the display from the offscreen buffer
        /// </summary>
        /// <param name="left">Left bounds in pixels</param>
        /// <param name="top">Top bounds in pixels</param>
        /// <param name="right">Right bounds in pixels</param>
        /// <param name="bottom">Bottom bounds in pixels</param>
        public void Show(int left, int top, int right, int bottom)
        {
            const int pageHeight = 8;

            //must update in pages (area of 128x8 pixels)
            //so iterate over all 8 pages and check if they're in range
            for (int page = 0; page < 8; page++)
            {
                if (top > pageHeight * page || bottom < (page + 1) * pageHeight)
                {
                    continue;
                }

                SendCommand((byte)((int)(DisplayCommand.PageAddress) | page));
                SendCommand((DisplayCommand.ColumnAddressLow) | (StartColumnOffset & 0x0F));
                SendCommand((int)DisplayCommand.ColumnAddressHigh | 0);

                dataCommandPort.State = Data;

                Array.Copy(imageBuffer.Buffer, Width * page, pageBuffer, 0, PageSize);
                spiComms.Write(pageBuffer);
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
                    resetPort?.Dispose();
                    dataCommandPort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}