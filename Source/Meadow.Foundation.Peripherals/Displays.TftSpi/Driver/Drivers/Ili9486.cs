using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a Ili9486 TFT color display
    /// </summary>
    public class Ili9486 : TftSpiBase, IRotatableDisplay
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
        /// Create a new Ili9486 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public Ili9486(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width = 320, int height = 480, ColorMode colorMode = ColorMode.Format12bppRgb444)
            : base(spiBus, chipSelectPin, dcPin, resetPin, width, height, colorMode)
        {
            Initialize();

            SetRotation(RotationType.Normal);
        }

        /// <summary>
        /// Create a new Ili9486 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public Ili9486(ISpiBus spiBus, IDigitalOutputPort chipSelectPort,
                IDigitalOutputPort dataCommandPort, IDigitalOutputPort resetPort,
                int width = 320, int height = 480, ColorMode colorMode = ColorMode.Format12bppRgb444) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, width, height, colorMode)
        {
            Initialize();

            SetRotation(RotationType.Normal);
        }

        /// <summary>
        /// Initialize the display
        /// </summary>
        protected override void Initialize()
        {
            SendCommand(Register.SLPOUT); // Sleep out, also SW reset
            DelayMs(120);

            SendCommand(Register.COLOR_MODE);
            if (ColorMode == ColorMode.Format16bppRgb565)
                SendData(0x55);
            else
                SendData(0x53);

            SendCommand(0xC2);
            SendData(0x44);

            SendCommand(0xC5);
            SendData(0x00);
            SendData(0x00);
            SendData(0x00);
            SendData(0x00);

            SendCommand(0xE0);
            SendData(0x0F);
            SendData(0x1F);
            SendData(0x1C);
            SendData(0x0C);
            SendData(0x0F);
            SendData(0x08);
            SendData(0x48);
            SendData(0x98);
            SendData(0x37);
            SendData(0x0A);
            SendData(0x13);
            SendData(0x04);
            SendData(0x11);
            SendData(0x0D);
            SendData(0x00);

            SendCommand(0xE1);
            SendData(0x0F);
            SendData(0x32);
            SendData(0x2E);
            SendData(0x0B);
            SendData(0x0D);
            SendData(0x05);
            SendData(0x47);
            SendData(0x75);
            SendData(0x37);
            SendData(0x06);
            SendData(0x10);
            SendData(0x03);
            SendData(0x24);
            SendData(0x20);
            SendData(0x00);

            SendCommand(Register.INVOFF);                     // display inversion OFF

            SendCommand(Register.MADCTL);
            SendData(0x48);

            SendCommand(0x29);                     // display on
            DelayMs(150);
        }

        /// <summary>
        /// Set the display rotation
        /// </summary>
        /// <param name="rotation">The rotation value</param>
        public void SetRotation(RotationType rotation)
        {
            SendCommand(Register.MADCTL);

            switch (Rotation = rotation)
            {
                case RotationType.Normal:
                    SendData((byte)Register.MADCTL_MX | (byte)Register.MADCTL_BGR);
                    break;
                case RotationType._90Degrees:
                    SendData((byte)Register.MADCTL_MV | (byte)Register.MADCTL_BGR);
                    break;
                case RotationType._180Degrees:
                    SendData((byte)Register.MADCTL_BGR | (byte)Register.MADCTL_MY);
                    break;
                case RotationType._270Degrees:
                    SendData((byte)Register.MADCTL_BGR | (byte)Register.MADCTL_MV | (byte)Register.MADCTL_MX | (byte)Register.MADCTL_MY);
                    break;
            }

            UpdateBuffer();
        }
    }
}
