using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Ssd130x
{
    /// <summary>
    /// Provide an interface to the SSD1306 family of OLED displays
    /// </summary>
    public partial class Ssd1306 : Ssd130xBase
    {
        /// <summary>
        /// Create a new SSD1306 object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="displayType">Type of SSD1306 display (default = 128x64 pixel display)</param>
        public Ssd1306(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            DisplayType displayType = DisplayType.OLED128x64):
            this(spiBus, device.CreateDigitalOutputPort(chipSelectPin, false), device.CreateDigitalOutputPort(dcPin, true),
                device.CreateDigitalOutputPort(resetPin, false), displayType)
        {
        }

        /// <summary>
        /// Create a new Ssd1306 display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="displayType">Type of SSD1306 display (default = 128x64 pixel display)</param>
        public Ssd1306(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            DisplayType displayType = DisplayType.OLED128x64)
        {
            this.dataCommandPort = dataCommandPort;
            this.chipSelectPort = chipSelectPort;
            this.resetPort = resetPort;

            spiPeripheral = new SpiPeripheral(spiBus, chipSelectPort);

            connectionType = ConnectionType.SPI;

            InitSSD1306(displayType);
        }

        /// <summary>
        /// Create a new SSD1306 object using the default parameters for
        /// </summary>
        /// <param name="i2cBus">I2cBus connected to display</param>
        /// <param name="address">Address of the bus on the I2C display.</param>
        /// <param name="displayType">Type of SSD1306 display (default = 128x64 pixel display).</param>
        public Ssd1306(II2cBus i2cBus,
            byte address = (byte)Addresses.Default,
            DisplayType displayType = DisplayType.OLED128x64)
        {
            this.displayType = displayType;

            i2cPeripheral = new I2cPeripheral(i2cBus, address);

            connectionType = ConnectionType.I2C;

            InitSSD1306(displayType);
        }

        private void InitSSD1306(DisplayType displayType)
        {
            int width = 0, height = 0, xOffset, yOffset;

            switch (displayType)
            {
                case DisplayType.OLED128x64:
                    width = 128;
                    height = 64;
                    SendCommands(Oled128x64SetupSequence);
                    break;
                case DisplayType.OLED64x48:
                    width = 128;
                    height = 64;
                    xOffset = 32;
                    yOffset = 16;
                    SendCommands(Oled128x64SetupSequence);
                    break;
                case DisplayType.OLED72x40:
                    width = 72;
                    height = 40;
                    SendCommands(Oled72x40SetupSequence);
                    break;
                case DisplayType.OLED128x32:
                    width = 128;
                    height = 32;
                    SendCommands(Oled128x32SetupSequence);
                    break;
                case DisplayType.OLED96x16:
                    width = 96;
                    height = 16;
                    SendCommands(Oled96x16SetupSequence);
                    break;
            }

            //create buffers
            imageBuffer = new Buffer1bpp(width, height, PAGE_SIZE);
            readBuffer = new byte[imageBuffer.ByteCount];
            pageBuffer = new byte[PAGE_SIZE + 1];

            commandBuffer = new byte[2];

            showPreamble = new byte[] { 0x21, 0x00, (byte)(width - 1), 0x22, 0x00, (byte)(height / 8 - 1) };

            // Finally, put the display into a known state.
            InvertDisplay = false;
            Sleep = false;
            Contrast = 0xff;
            StopScrolling();
        }

        public override void Fill(Color clearColor, bool updateDisplay = false)
        {
            imageBuffer.Clear(clearColor.Color1bpp);

            if(updateDisplay)
            {
                Show();
            }
        }

        public override void Fill(int x, int y, int width, int height, Color color)
        {
            imageBuffer.Fill(x, y, width, height, color);
        }

        public override void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
        {
            imageBuffer.WriteBuffer(x, y, displayBuffer);
        }
    }
}