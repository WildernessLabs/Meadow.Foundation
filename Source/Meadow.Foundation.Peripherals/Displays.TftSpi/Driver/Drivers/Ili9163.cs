using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a Ili9163 TFT color display
    /// </summary>
    public class Ili9163 : TftSpiBase
    {
        /// <summary>
        /// The default display color mode
        /// </summary>
        public override ColorMode DefaultColorMode => ColorMode.Format12bppRgb444;

        /// <summary>
        /// The color modes supported by the display
        /// </summary>
        public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565 | ColorMode.Format12bppRgb444;

        /// <summary>
        /// Create a new Ili9163 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public Ili9163(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width, int height, ColorMode colorMode = ColorMode.Format12bppRgb444)
            : base(spiBus, chipSelectPin, dcPin, resetPin, width, height, colorMode)
        {
            Initialize();
        }

        /// <summary>
        /// Create a new Ili9163 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public Ili9163(ISpiBus spiBus, IDigitalOutputPort chipSelectPort,
                IDigitalOutputPort dataCommandPort, IDigitalOutputPort resetPort,
                int width, int height, ColorMode colorMode = ColorMode.Format12bppRgb444) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, width, height, colorMode)
        {
            Initialize();
        }

        /// <summary>
        /// Initialize the display
        /// </summary>
        protected override void Initialize()
        {
            if (resetPort != null)
            {
                resetPort.State = true;
                DelayMs(50);
                resetPort.State = false;
                DelayMs(50);
                resetPort.State = true;
                DelayMs(50);
            }
            else
            {
                DelayMs(150); //Not sure if this is needed but can't hurt
            }

            SendCommand(0x01);
            SendCommand(0x11);

            SendCommand(Register.COLOR_MODE);
            if (ColorMode == ColorMode.Format16bppRgb565)
            {
                SendData(0x05);//16 bit 565
            }
            else
            {
                SendData(0x03);
            }

            SendCommand(0x26);
            SendData(0x04);

            SendCommand(0x36);
            SendData(0x00); //RGB

            SendCommand(0xF2);
            SendData(0x01);

            SendCommand(0xE0);
            dataCommandPort.State = Data;
            Write(0x04);
            Write(0x3F);
            Write(0x25);
            Write(0x1C);
            Write(0x1E);
            Write(0x20);
            Write(0x12);
            Write(0x2A);
            Write(0x90);
            Write(0x24);
            Write(0x11);
            Write(0x00);
            Write(0x00);
            Write(0x00);
            Write(0x00);
            Write(0x00); // Positive Gamma

            SendCommand(0xE1);
            dataCommandPort.State = (Data);
            Write(0x20);
            Write(0x20);
            Write(0x20);
            Write(0x20);
            Write(0x05);
            Write(0x00);
            Write(0x15);
            Write(0xA7);
            Write(0x3D);
            Write(0x18);
            Write(0x25);
            Write(0x2A);
            Write(0x2B);
            Write(0x2B);
            Write(0x3A); // Negative Gamma

            SendCommand(0xB1);
            dataCommandPort.State = (Data);
            Write(0x08);
            Write(0x08); // Frame rate control 1

            SendCommand(0xB4);
            dataCommandPort.State = (Data);
            Write(0x07);      // Display inversion

            SendCommand(0xC0);
            dataCommandPort.State = (Data);
            Write(0x0A);
            Write(0x02); // Power control 1

            SendCommand(0xC1);
            dataCommandPort.State = (Data);
            Write(0x02);       // Power control 2

            dataCommandPort.State = (Command);
            Write(0xC5);
            dataCommandPort.State = (Data);
            Write(0x50);
            Write(0x5B); // Vcom control 1

            dataCommandPort.State = (Command);
            Write(0xC7);
            dataCommandPort.State = (Data);
            Write(0x40);       // Vcom offset

            dataCommandPort.State = (Command);
            Write(0x2A);
            dataCommandPort.State = (Data);
            Write(0x00);
            Write(0x00);
            Write(0x00);
            Write(0x7F);
            DelayMs(250); // Set column address

            dataCommandPort.State = (Command);
            Write(0x2B);
            dataCommandPort.State = (Data);
            Write(0x00);
            Write(0x00);
            Write(0x00);
            Write(0x9F);           // Set page address

            dataCommandPort.State = (Command);
            Write(0x36);
            dataCommandPort.State = (Data);
            Write(0xC0);       // Set address mode

            dataCommandPort.State = (Command);
            Write(0x29);           // Set display on
            DelayMs(10);

            dataCommandPort.State = Data;
        }
    }
}