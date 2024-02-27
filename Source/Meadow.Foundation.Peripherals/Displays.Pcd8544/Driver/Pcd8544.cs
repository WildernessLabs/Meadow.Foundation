using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a Pcd8544 monochrome display
    /// </summary>
    public class Pcd8544 : IPixelDisplay, ISpiPeripheral, IDisposable
    {
        /// <inheritdoc/>
        public ColorMode ColorMode => ColorMode.Format1bpp;

        /// <inheritdoc/>
        public ColorMode SupportedColorModes => ColorMode.Format1bpp;

        /// <inheritdoc/>
        public int Height => 48;

        /// <inheritdoc/>
        public int Width => 84;

        /// <inheritdoc/>
        public IPixelBuffer PixelBuffer => imageBuffer;

        /// <summary>
        /// Is the display inverted 
        /// </summary>
        public bool IsDisplayInverted { get; private set; } = false;

        /// <summary>
        /// The default SPI bus speed for the device
        /// </summary>
        public Frequency DefaultSpiBusSpeed => new Frequency(4000, Frequency.UnitType.Kilohertz);

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
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPorts = false;

        readonly IDigitalOutputPort dataCommandPort;
        readonly IDigitalOutputPort resetPort;
        readonly IDigitalOutputPort? chipSelectPort;

        /// <summary>
        /// SPI Communication bus used to communicate with the peripheral
        /// </summary>
        protected ISpiCommunications spiComms;

        /// <summary>
        /// Buffer to hold display data
        /// </summary>
        protected Buffer1bpp imageBuffer;

        /// <summary>
        /// Buffer to hold internal command data to be sent over the SPI bus
        /// </summary>
        protected Memory<byte> commandBuffer;

        /// <summary>
        /// Create a Pcd8544 object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        public Pcd8544(ISpiBus spiBus, IPin? chipSelectPin, IPin dcPin, IPin resetPin) :
            this(spiBus, chipSelectPin?.CreateDigitalOutputPort() ?? null, dcPin.CreateDigitalOutputPort(true),
                resetPin.CreateDigitalOutputPort(true))
        {
            createdPorts = true;
        }

        /// <summary>
        /// Create a new Pcd8544 display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        public Pcd8544(ISpiBus spiBus,
            IDigitalOutputPort? chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort)
        {
            imageBuffer = new Buffer1bpp(Width, Height);

            dataCommandPort.State = true;
            resetPort.State = true;

            this.dataCommandPort = dataCommandPort;
            this.resetPort = resetPort;

            spiComms = new SpiCommunications(spiBus, this.chipSelectPort = chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

            Initialize();
        }

        private void Initialize()
        {
            resetPort.State = false;
            resetPort.State = true;

            dataCommandPort.State = false;

            commandBuffer.Span[0] = 0x21;
            commandBuffer.Span[1] = 0xBF;
            commandBuffer.Span[2] = 0x04;
            commandBuffer.Span[3] = 0x14;
            commandBuffer.Span[4] = 0x0D;
            commandBuffer.Span[5] = 0x20;
            commandBuffer.Span[6] = 0x0C;

            spiComms.Write(commandBuffer.Span[0..6]);

            dataCommandPort.State = true;

            Clear();
            Show();
        }

        /// <summary>
        /// Clear the display
        /// </summary>
        /// <remarks>
        /// Clears the internal memory buffer 
        /// </remarks>
        /// <param name="updateDisplay">If true, it will force a display update</param>
        public void Clear(bool updateDisplay = false)
        {
            Array.Clear(imageBuffer.Buffer, 0, imageBuffer.ByteCount);

            if (updateDisplay)
            {
                Show();
            }
        }

        /// <summary>
        /// Draw pixel at location
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        /// <param name="enabled">True = turn on pixel, false = turn off pixel</param>
        public void DrawPixel(int x, int y, bool enabled)
        {
            imageBuffer.SetPixel(x, y, enabled);
        }

        /// <summary>
        /// Invert pixel at a location
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        public void InvertPixel(int x, int y)
        {
            imageBuffer.InvertPixel(x, y);
        }

        /// <summary>
        /// Draw pixel at location
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        /// <param name="color">any value other than black will make the pixel visible</param>
        public void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color1bpp);
        }

        /// <summary>
        /// Update the display
        /// </summary>
        public void Show()
        {
            spiComms.Write(imageBuffer.Buffer);
        }

        /// <summary>
        /// Update a region of the display 
        /// Currently always does a full screen update for this display
        /// </summary>
        /// <param name="left">The left position in pixels</param>
        /// <param name="top">The top position in pixels</param>
        /// <param name="right">The right position in pixels</param>
        /// <param name="bottom">The bottom position in pixels</param>
        public void Show(int left, int top, int right, int bottom)
        {   //ToDo implement partial screen updates for PCD8544
            Show();
        }

        /// <summary>
        /// Invert the entire display
        /// </summary>
        /// <param name="inverse">Invert if true, normal if false</param>
        public void InvertDisplay(bool inverse)
        {
            IsDisplayInverted = inverse;
            dataCommandPort.State = false;
            commandBuffer.Span[0] = inverse ? (byte)0x0D : (byte)0x0C;

            spiComms.Write(commandBuffer.Span[0]);
            dataCommandPort.State = true;
        }

        /// <summary>
        /// Fill with color 
        /// </summary>
        /// <param name="fillColor">color - converted to on/off</param>
        /// <param name="updateDisplay">should refresh display</param>
        public void Fill(Color fillColor, bool updateDisplay = false)
        {
            imageBuffer.Clear(fillColor.Color1bpp);

            if (updateDisplay) { Show(); }
        }

        /// <summary>
        /// Fill region with color
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="width">width of region</param>
        /// <param name="height">height of region</param>
        /// <param name="fillColor">color - converted to on/off</param>
        public void Fill(int x, int y, int width, int height, Color fillColor)
        {
            imageBuffer.Fill(x, y, width, height, fillColor);
        }

        /// <summary>
        /// Draw buffer at location
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        /// <param name="displayBuffer">buffer to draw</param>
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
                    dataCommandPort?.Dispose();
                    resetPort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}