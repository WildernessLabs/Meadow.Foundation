using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a WaveShare 5.65" ACeP 7 color e-Paper display
    /// </summary>
    public class Epd5in65f : EPaperBase, IPixelDisplay
    {
        /// <inheritdoc/>
        public ColorMode ColorMode => ColorMode.Format4bppIndexed;

        /// <inheritdoc/>
        public ColorMode SupportedColorModes => ColorMode.Format4bppIndexed;

        /// <inheritdoc/>
        public IPixelBuffer PixelBuffer => imageBuffer;

        /// <inheritdoc/>
        public Color EnabledColor => Color.Black;

        /// <inheritdoc/>
        public Color DisabledColor => Color.White;

        /// <summary>
        /// Buffer to hold display data
        /// </summary>
        protected readonly BufferIndexed4 imageBuffer;

        /// <inheritdoc/>
        public int Width => 600;

        /// <inheritdoc/>
        public int Height => 448;

        /// <summary>
        /// Create a new Epd5in65f ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        public Epd5in65f(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin) :
            this(spiBus,
                chipSelectPin.CreateDigitalOutputPort(),
                dcPin.CreateDigitalOutputPort(),
                resetPin.CreateDigitalOutputPort(),
                busyPin.CreateDigitalInputPort())
        {
        }

        /// <summary>
        /// Create a new Epd5in65f ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        public Epd5in65f(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            IDigitalInputPort busyPort)
        {
            this.dataCommandPort = dataCommandPort;
            this.chipSelectPort = chipSelectPort;
            this.resetPort = resetPort;
            this.busyPort = busyPort;

            spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

            imageBuffer = new BufferIndexed4(Width, Height);

            //set the indexed colors
            imageBuffer.IndexedColors[0] = Color.Black;
            imageBuffer.IndexedColors[1] = Color.White;
            imageBuffer.IndexedColors[2] = Color.Green;
            imageBuffer.IndexedColors[3] = Color.Blue;
            imageBuffer.IndexedColors[4] = Color.Red;
            imageBuffer.IndexedColors[5] = Color.Yellow;
            imageBuffer.IndexedColors[6] = Color.Orange;

            imageBuffer.Clear();

            Initialize();
        }

        void Initialize()
        {
            Reset();
            WaitForBusyState(true);
            SendCommand(0x00);
            SendData(0xEF);
            SendData(0x08);
            SendCommand(0x01);
            SendData(0x37);
            SendData(0x00);
            SendData(0x23);
            SendData(0x23);
            SendCommand(0x03);
            SendData(0x00);
            SendCommand(0x06);
            SendData(0xC7);
            SendData(0xC7);
            SendData(0x1D);
            SendCommand(0x30);
            SendData(0x3C);
            SendCommand(0x41);
            SendData(0x00);
            SendCommand(0x50);
            SendData(0x37);
            SendCommand(0x60);
            SendData(0x22);
            SendCommand(0x61);
            SendData(0x02);
            SendData(0x58);
            SendData(0x01);
            SendData(0xC0);
            SendCommand(0xE3);
            SendData(0xAA);

            DelayMs(100);
            SendCommand(0x50);
            SendData(0x37);
        }

        /// <summary>
        /// Clear display buffer
        /// </summary>
        /// <param name="updateDisplay">force display update</param>
        public void Clear(bool updateDisplay = false)
        {
            imageBuffer.Clear();

            if (updateDisplay)
            {
                Show();
            }
        }

        /// <summary>
        /// Clear the display
        /// </summary>
        /// <param name="color">Color to set the display</param>
        public void Fill(Color color)
        {
            imageBuffer.Fill(color);
        }

        /// <summary>
        /// Fill the display buffer with a color
        /// </summary>
        /// <param name="x">x location in pixels to start fill</param>
        /// <param name="y">y location in pixels to start fill</param>
        /// <param name="width">width in pixels to fill</param>
        /// <param name="height">height in pixels to fill</param>
        /// <param name="color">color to fill</param>
        public void Fill(int x, int y, int width, int height, Color color)
        {
            imageBuffer.Fill(x, y, width, height, color);
        }

        /// <summary>
        /// Clear the display
        /// </summary>
        public void Clear()
        {
            imageBuffer.Clear();
        }

        /// <summary>
        /// Draw the display buffer to screen
        /// </summary>
        public void Show()
        {
            SendCommand(0x61); //Set the resolution
            SendData(0x02);
            SendData(0x58);
            SendData(0x01);
            SendData(0xC0);
            SendCommand(0x10);

            dataCommandPort!.State = DataState;

            spiComms!.Write(imageBuffer.Buffer);

            SendCommand(0x04);
            WaitForBusyState(true);

            SendCommand(0x12);
            WaitForBusyState(true);

            SendCommand(0x02);
            WaitForBusyState(false);
            DelayMs(200);
        }

        /// <summary>
        /// Transfer part of the contents of the buffer to the display
        /// bounded by left, top, right and bottom
        /// </summary>
        public void Show(int left, int top, int right, int bottom)
        {
            SendCommand(0x61); //Set the resolution
            SendData(0x02);
            SendData(0x58);
            SendData(0x01);
            SendData(0xC0);
            SendCommand(0x10);

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width / 2; j++)
                {
                    if (i < bottom && i >= top && j < right / 2 && j >= left / 2)
                    {
                        spiComms!.Write(imageBuffer.Buffer[j + ((Width / 2) * i)]);
                    }
                    else
                    {   //no-op 
                        SendData(0x11);
                    }
                }
            }
            SendCommand(0x04);
            WaitForBusyState(true);
            SendCommand(0x12);
            WaitForBusyState(true);
            SendCommand(0x02);
            WaitForBusyState(false);
            DelayMs(200);
        }

        /// <summary>
        /// Clear the display
        /// </summary>
        /// <param name="fillColor">The color used to fill the display buffer</param>
        /// <param name="updateDisplay">Update the display once the buffer has been cleared when true</param>
        public void Fill(Color fillColor, bool updateDisplay = false)
        {
            Fill(fillColor);

            if (updateDisplay)
            {
                Show();
            }
        }

        /// <summary>
        /// Draw a single pixel at the specified color
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        /// <param name="color">The Meadow Foundation color of the pixel</param>
        public void DrawPixel(int x, int y, Color color)
        {
            imageBuffer.SetPixel(x, y, color);
        }

        /// <summary>
        /// Enable or disable a single pixel (used for 1bpp displays)
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        /// <param name="enabled">On if true, off if false</param>
        public void DrawPixel(int x, int y, bool enabled)
        {
            imageBuffer.SetPixel(x, y, enabled ? EnabledColor : DisabledColor);
        }

        /// <summary>
        /// Invert the color of a single pixel
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        public void InvertPixel(int x, int y)
        {
            imageBuffer.InvertPixel(x, y);
        }

        /// <summary>
        /// Draw a buffer to the display
        /// </summary>
        public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
        {
            imageBuffer.WriteBuffer(x, y, displayBuffer);
        }

        /// <summary>
        /// Wait until the display busy state is set
        /// </summary>
        protected virtual void WaitForBusyState(bool state)
        {
            int count = 0;

            if (busyPort is null)
            {
                DelayMs(200);
                return;
            }

            while (busyPort.State != state && count < 20)
            {
                DelayMs(50);
                count++;
            }
        }

        /// <summary>
        /// Enter deep sleep mode
        /// call reset to wake display
        /// </summary>
        public void Sleep()
        {
            DelayMs(100);
            SendCommand(0x07);
            SendData(0xA5);
            DelayMs(100);
            resetPort!.State = false;
        }
    }
}