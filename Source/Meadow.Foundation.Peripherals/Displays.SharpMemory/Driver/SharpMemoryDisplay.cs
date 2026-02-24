using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a Sharp Memory Display (LS013B4DN04, LS027B7DH01, LS032B1L03 and similar)
    /// </summary>
    /// <remarks>
    /// Sharp Memory Displays use a unique SPI protocol with an active-high chip select
    /// and a VCOM bit that must be toggled with each refresh to prevent DC bias damage.
    /// The display has no DC or reset pins - only SPI bus and CS are required.
    ///
    /// Supported display sizes:
    ///   LS013B4DN04:  96 x  96 (default)
    ///   LS027B7DH01: 400 x 240
    ///   LS032B1L03:  320 x 240
    /// </remarks>
    public class SharpMemoryDisplay : IPixelDisplay, ISpiPeripheral, IDisposable
    {
        // Command bytes for MSB-first SPI (bit-reversed from Sharp's LSB-first protocol)
        private const byte SHARP_CMD_WRITE = 0x80; // 0x01 bit-reversed: write framebuffer command
        private const byte SHARP_CMD_VCOM  = 0x40; // 0x02 bit-reversed: VCOM alternation bit
        private const byte SHARP_CMD_CLEAR = 0x20; // 0x04 bit-reversed: hardware clear command

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
        public Frequency DefaultSpiBusSpeed => new Frequency(2000, Frequency.UnitType.Kilohertz);

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

        private readonly bool createdPort = false;
        private readonly IDigitalOutputPort? chipSelectPort;

        /// <summary>
        /// SPI communications bus used to communicate with the peripheral
        /// </summary>
        protected ISpiCommunications spiComms;

        /// <summary>
        /// Pixel buffer for the display (horizontal 1bpp packing, MSB = leftmost pixel)
        /// </summary>
        protected Buffer1bpp imageBuffer;

        // Pre-allocated SPI frame buffer to avoid per-refresh heap allocation
        private readonly byte[] spiFrameBuffer;
        private readonly int bytesPerRow;

        // VCOM alternation bit - must toggle with every refresh to prevent DC bias
        private byte vcomBit = SHARP_CMD_VCOM;

        /// <summary>
        /// Creates a new SharpMemoryDisplay driver
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin (active high on this display)</param>
        /// <param name="width">Display width in pixels (default 96)</param>
        /// <param name="height">Display height in pixels (default 96)</param>
        public SharpMemoryDisplay(ISpiBus spiBus, IPin chipSelectPin, int width = 96, int height = 96)
            : this(spiBus, chipSelectPin.CreateDigitalOutputPort(false), width, height)
        {
            createdPort = true;
        }

        /// <summary>
        /// Creates a new SharpMemoryDisplay driver
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port (active high on this display)</param>
        /// <param name="width">Display width in pixels (default 96)</param>
        /// <param name="height">Display height in pixels (default 96)</param>
        public SharpMemoryDisplay(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, int width = 96, int height = 96)
        {
            this.chipSelectPort = chipSelectPort;
            chipSelectPort.State = false; // CS idle LOW (active-high device)

            // Pass null for CS - we drive it manually because ActiveHigh support
            // varies across platform implementations
            spiComms = new SpiCommunications(spiBus, null, DefaultSpiBusSpeed, DefaultSpiBusMode);

            imageBuffer = new Buffer1bppV(width, height);

            bytesPerRow = width / 8;

            // Frame layout: 1 cmd + height*(1 addr + bytesPerRow data + 1 trailing) + 1 final trailing
            spiFrameBuffer = new byte[2 + height * (2 + bytesPerRow)];

            // Pre-fill row address bytes (bit-reversed: Sharp is LSB-first, Meadow SPI is MSB-first)
            for (int row = 1; row <= height; row++)
            {
                spiFrameBuffer[1 + (row - 1) * (2 + bytesPerRow)] = ReverseBits((byte)row);
            }

            // Start with a cleared white display
            imageBuffer.Clear(true);
            ClearDisplay();
        }

        /// <summary>
        /// Reverses the bits in a byte (converts between LSB-first and MSB-first bit ordering)
        /// </summary>
        private static byte ReverseBits(byte b)
        {
            b = (byte)((b & 0xF0) >> 4 | (b & 0x0F) << 4);
            b = (byte)((b & 0xCC) >> 2 | (b & 0x33) << 2);
            b = (byte)((b & 0xAA) >> 1 | (b & 0x55) << 1);
            return b;
        }

        private void ToggleVcom()
        {
            vcomBit = (vcomBit == 0) ? SHARP_CMD_VCOM : (byte)0;
        }

        /// <summary>
        /// Clears the pixel buffer to white (the display's natural resting state)
        /// </summary>
        /// <param name="updateDisplay">If true, calls Show() to push the cleared buffer to the display</param>
        public void Clear(bool updateDisplay = false)
        {
            imageBuffer.Clear(true); // 1 = white on Sharp Memory Display

            if (updateDisplay)
            {
                Show();
            }
        }

        /// <summary>
        /// Sends a hardware clear command to the display (faster than a full buffer refresh).
        /// Also resets the pixel buffer to white.
        /// </summary>
        public void ClearDisplay()
        {
            imageBuffer.Clear(true);

            Span<byte> cmd = stackalloc byte[] { (byte)(SHARP_CMD_CLEAR | vcomBit), 0x00 };
            chipSelectPort!.State = true;
            spiComms.Write(cmd);
            chipSelectPort.State = false;

            ToggleVcom();
        }

        /// <summary>
        /// Draw a pixel at the specified location
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        /// <param name="enabled">true = white pixel, false = black pixel</param>
        public void DrawPixel(int x, int y, bool enabled)
        {
            imageBuffer.SetPixel(x, y, enabled);
        }

        /// <summary>
        /// Draw a pixel at the specified location
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        /// <param name="color">color converted to on/off (white or black)</param>
        public void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color1bpp);
        }

        /// <summary>
        /// Invert a pixel at the specified location
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        public void InvertPixel(int x, int y)
        {
            imageBuffer.InvertPixel(x, y);
        }

        /// <summary>
        /// Send the full pixel buffer to the display
        /// </summary>
        public void Show()
        {
            spiFrameBuffer[0] = (byte)(SHARP_CMD_WRITE | vcomBit);
            ToggleVcom();

            for (int row = 1; row <= Height; row++)
            {
                int frameOffset = 1 + (row - 1) * (2 + bytesPerRow);
                int bufferOffset = (row - 1) * bytesPerRow;
                // spiFrameBuffer[frameOffset] = row address (pre-filled in constructor)
                Array.Copy(imageBuffer.Buffer, bufferOffset, spiFrameBuffer, frameOffset + 1, bytesPerRow);
                // spiFrameBuffer[frameOffset + 1 + bytesPerRow] = 0x00 trailing (pre-zeroed)
            }
            // spiFrameBuffer[last] = 0x00 final trailing (pre-zeroed)

            chipSelectPort!.State = true;
            spiComms.Write(spiFrameBuffer);
            chipSelectPort.State = false;
        }

        /// <summary>
        /// Send a partial update to the display (only rows within the specified bounds are sent)
        /// </summary>
        /// <param name="left">Left boundary (unused - full rows are always transmitted)</param>
        /// <param name="top">Top row boundary in pixels</param>
        /// <param name="right">Right boundary (unused - full rows are always transmitted)</param>
        /// <param name="bottom">Bottom row boundary in pixels</param>
        public void Show(int left, int top, int right, int bottom)
        {
            top = Math.Max(0, top);
            bottom = Math.Min(Height - 1, bottom);

            if (top > bottom) return;

            int rowCount = bottom - top + 1;
            var frame = new byte[2 + rowCount * (2 + bytesPerRow)];

            frame[0] = (byte)(SHARP_CMD_WRITE | vcomBit);
            ToggleVcom();

            for (int i = 0; i < rowCount; i++)
            {
                int row = top + i + 1; // Sharp row addresses are 1-indexed
                int frameOffset = 1 + i * (2 + bytesPerRow);
                frame[frameOffset] = ReverseBits((byte)row);
                Array.Copy(imageBuffer.Buffer, (top + i) * bytesPerRow, frame, frameOffset + 1, bytesPerRow);
                // frame[frameOffset + 1 + bytesPerRow] = 0x00 trailing (pre-zeroed)
            }
            // frame[last] = 0x00 final trailing (pre-zeroed)

            chipSelectPort!.State = true;
            spiComms.Write(frame);
            chipSelectPort.State = false;
        }

        /// <summary>
        /// Fill the entire display with a color
        /// </summary>
        /// <param name="fillColor">Color to fill (converted to white or black)</param>
        /// <param name="updateDisplay">If true, calls Show() after filling</param>
        public void Fill(Color fillColor, bool updateDisplay = false)
        {
            imageBuffer.Clear(fillColor.Color1bpp);

            if (updateDisplay) Show();
        }

        /// <summary>
        /// Fill a region of the display with a color
        /// </summary>
        /// <param name="x">x position of the fill region</param>
        /// <param name="y">y position of the fill region</param>
        /// <param name="width">Width of the fill region in pixels</param>
        /// <param name="height">Height of the fill region in pixels</param>
        /// <param name="fillColor">Color to fill (converted to white or black)</param>
        public void Fill(int x, int y, int width, int height, Color fillColor)
        {
            imageBuffer.Fill(x, y, width, height, fillColor);
        }

        /// <summary>
        /// Write a pixel buffer to a location on the display
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        /// <param name="displayBuffer">Buffer to write</param>
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
                if (disposing && createdPort)
                {
                    chipSelectPort?.Dispose();
                }
                IsDisposed = true;
            }
        }
    }
}
