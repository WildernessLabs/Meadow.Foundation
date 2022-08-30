using Meadow.Hardware;
using Meadow.Foundation.Graphics.Buffers;
using System;
using Meadow.Foundation.Graphics;
using Meadow.Units;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a Pcd8544 monochrome display
    /// </summary>
    public class Pcd8544 : IGraphicsDisplay
    {
        /// <summary>
        /// Default SPI bus speed
        /// </summary>
        public static Frequency DEFAULT_SPEED = new Frequency(4000, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// Display color mode 
        /// </summary>
        public ColorType ColorMode => ColorType.Format1bpp;

        /// <summary>
        /// Height of display in pixels
        /// </summary>
        public int Height => 48;

        /// <summary>
        /// Width of display in pixels
        /// </summary>
        public int Width => 84;

        /// <summary>
        /// The buffer the holds the pixel data for the display
        /// </summary>
        public IPixelBuffer PixelBuffer => imageBuffer;

        /// <summary>
        /// Is the display inverted 
        /// </summary>
        public bool IsDisplayInverted { get; private set; } = false;

        protected IDigitalOutputPort dataCommandPort;
        protected IDigitalOutputPort resetPort;
        protected IDigitalOutputPort chipSelectPort;
        protected ISpiPeripheral spiPeripheral;

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
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        public Pcd8544(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin):
            this(spiBus, device.CreateDigitalOutputPort(chipSelectPin), device.CreateDigitalOutputPort(dcPin, true),
                device.CreateDigitalOutputPort(resetPin, true))
        {
        }

        /// <summary>
        /// Create a new Pcd8544 display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        public Pcd8544(ISpiBus spiBus, 
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort)
        {
            dataCommandPort.State = true;
            resetPort.State = true;

            this.dataCommandPort = dataCommandPort;
            this.chipSelectPort = chipSelectPort;
            this.resetPort = resetPort;

            spiPeripheral = new SpiPeripheral(spiBus, chipSelectPort);

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

            spiPeripheral.Write(commandBuffer.Span[0..6]);

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

        public void Show()
        {
            spiPeripheral.Write(imageBuffer.Buffer);
        }

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

            spiPeripheral.Write(commandBuffer.Span[0]);
            dataCommandPort.State = true;
        }

        public void Fill(Color clearColor, bool updateDisplay = false)
        {
            imageBuffer.Clear(clearColor.Color1bpp);

            if(updateDisplay) { Show(); }
        }

        public void Fill(int x, int y, int width, int height, Color color)
        {
            imageBuffer.Fill(x, y, width, height, color);
        }

        public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
        {
            imageBuffer.WriteBuffer(x, y, displayBuffer);
        }
    }
}