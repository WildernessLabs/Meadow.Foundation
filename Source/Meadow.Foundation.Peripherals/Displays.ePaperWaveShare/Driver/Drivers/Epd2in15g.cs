using Meadow;
using Meadow.Foundation;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Displays.ePaperWaveShare.Drivers
{
    /// <summary>
    /// Represents a WaveShare color ePaper color display
    /// 296x160, 2.15" e-Ink four-color display, SPI interface 
    /// </summary>
    public class Epd2in15g : EPaperBase, IPixelDisplay
    {
        /// <inheritdoc/>
        public ColorMode ColorMode => ColorMode.Format2bppIndexed;

        /// <inheritdoc/>
        public ColorMode SupportedColorModes => ColorMode.Format2bppIndexed;

        /// <inheritdoc/>
        public IPixelBuffer PixelBuffer => imageBuffer;

        /// <inheritdoc/>
        public Color EnabledColor => Color.Black;

        /// <inheritdoc/>
        public Color DisabledColor => Color.White;

        /// <summary>
        /// Buffer to hold display data
        /// </summary>
        protected readonly BufferIndexed2 imageBuffer;

        /// <inheritdoc/>
        public int Width => 160;

        /// <inheritdoc/>
        public int Height => 296;

        /// <summary>
        /// Create a new Epd2in15g ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        public Epd2in15g(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin) :
            this(spiBus,
                chipSelectPin.CreateDigitalOutputPort(),
                dcPin.CreateDigitalOutputPort(),
                resetPin.CreateDigitalOutputPort(),
                busyPin.CreateDigitalInputPort())
        {
        }

        /// <summary>
        /// Create a new Epd2in15g ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        public Epd2in15g(ISpiBus spiBus,
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

            imageBuffer = new BufferIndexed2(Width, Height);

            //set the indexed colors
            imageBuffer.IndexedColors[0] = Color.Black;
            imageBuffer.IndexedColors[1] = Color.White;
            imageBuffer.IndexedColors[2] = Color.Yellow;
            imageBuffer.IndexedColors[3] = Color.Red;

            imageBuffer.Clear();

            Initialize();
        }

        void Initialize()
        {
            Reset();

            SendCommand(0x4D);
            SendData(0x78);

            SendCommand(0x00);   //0x00
            SendData(0x0F);
            SendData(0x29);  // D5=0

            SendCommand(0x01);   //0x01
            SendData(0x07);
            SendData(0x00);

            SendCommand(0x03);   //0x03
            SendData(0x10);
            SendData(0x54);
            SendData(0x44);

            SendCommand(0x06);   //0x06
            SendData(0x0F);
            SendData(0x0A);
            SendData(0x2F);
            SendData(0x25);
            SendData(0x22);
            SendData(0x2E);
            SendData(0x21);

            SendCommand(0x30);
            SendData(0x02);

            SendCommand(0x41);   //0x41
            SendData(0x00);

            SendCommand(0x50);   //0x50
            SendData(0x37);

            SendCommand(0x60);   //0x60
            SendData(0x02);
            SendData(0x02);

            SendCommand(0x61); //0x61
            SendData(Width / 256);
            SendData(Width % 256);
            SendData(Height / 256);
            SendData(Height % 256);

            SendCommand(0x65);   //0x65
            SendData(0x00);
            SendData(0x00);
            SendData(0x00);
            SendData(0x00);

            SendCommand(0XE7);   // 0XE7
            SendData(0x1C);

            SendCommand(0xE3);   //0xE3
            SendData(0x22);

            SendCommand(0xE0);   //0xE0
            SendData(0x00);

            SendCommand(0xB4);
            SendData(0xD0);
            SendCommand(0xB5);
            SendData(0x03);

            SendCommand(0xE9);
            SendData(0x01);

            SendCommand(0x04);
            WaitForBusyState(true);
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
        /// <param name="width">w in pixels to fill</param>
        /// <param name="height">h in pixels to fill</param>
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
            var w = (Width % 4 == 0) ? (Width / 4) : (Width / 4 + 1);
            var h = Height;

            SendCommand(0x10);
            for (int j = 0; j < h; j++)
            {
                for (int i = 0; i < w; i++)
                {
                    SendData(PixelBuffer.Buffer[i + j * w]);
                    //SendData(0xFF); //all red                     11 (3)
                    //SendData(0x00); //black                       00 (0)
                    //SendData(0x01); //thin white thick black
                    //SendData(0x55);// 0101 0101 all white         01 (1)
                    //SendData(0xAA); // 1010 1010 all yellow         10 (2)
                }
            }

            DisplayOn();
        }

        void DisplayOn()
        {
            SendCommand(0x12);
            SendData(0x00);
            WaitForBusyState(true);
        }

        /// <summary>
        /// Transfer part of the contents of the buffer to the display
        /// bounded by left, top, right and bottom
        /// </summary>
        public void Show(int left, int top, int right, int bottom)
        {
            Show();
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
                DelayMs(1000);
                return;
            }

            while (busyPort.State != state && count < 1000)
            {
                DelayMs(5);
                count++;
            }
        }

        /// <summary>
        /// Reset the display
        /// </summary>
        protected override void Reset()
        {
            if (resetPort != null)
            {
                resetPort.State = true;
                DelayMs(200);
                resetPort.State = false;
                DelayMs(2);
                resetPort.State = true;
                DelayMs(200);
            }
        }

        /// <summary>
        /// Enter deep sleep mode
        /// call reset to wake display
        /// </summary>
        public void Sleep()
        {
            SendCommand(0x02); // POWER_OFF
            SendData(0X00);
            SendCommand(0x07); // DEEP_SLEEP
            SendData(0XA5);
        }
    }
}