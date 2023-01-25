using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a Ili9488 TFT color display
    /// </summary>
    public class Ili9488 : TftSpiBase
    {
        /// <summary>
        /// The default display color mode
        /// </summary>
        public override ColorType DefautColorMode => ColorType.Format24bppRgb888;

        /// <summary>
        /// Create a new Ili9488 display object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Ili9488(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width = 320, int height = 480) 
            : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height, ColorType.Format24bppRgb888)
        {
            Initialize();

            SetRotation(RotationType.Normal);
        }

        /// <summary>
        /// Create a new Ili9488 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public Ili9488(ISpiBus spiBus, IDigitalOutputPort chipSelectPort,
                IDigitalOutputPort dataCommandPort, IDigitalOutputPort resetPort,
                int width = 320, int height = 480) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, width, height, ColorType.Format24bppRgb888)
        {
            Initialize();

            SetRotation(RotationType.Normal);
        }

        /// <summary>
        /// Is the color mode supported by the display
        /// </summary>
        /// <param name="mode">The color mode</param>
        /// <returns>True if supported</returns>
        public override bool IsColorModeSupported(ColorType mode)
        {
            return mode == ColorType.Format24bppRgb888;
        }

        /// <summary>
        /// Initalize the display
        /// </summary>
        protected override void Initialize()
        {
            resetPort.State = true;
            DelayMs(5);
            resetPort.State = false;
            DelayMs(120);
            resetPort.State = true;
            DelayMs(150);

            SendCommand(0xE0); // Positive Gamma Control
            SendData(0x00);
            SendData(0x03);
            SendData(0x09);
            SendData(0x08);
            SendData(0x16);
            SendData(0x0A);
            SendData(0x3F);
            SendData(0x78);
            SendData(0x4C);
            SendData(0x09);
            SendData(0x0A);
            SendData(0x08);
            SendData(0x16);
            SendData(0x1A);
            SendData(0x0F);

            SendCommand(0XE1); // Negative Gamma Control
            SendData(0x00);
            SendData(0x16);
            SendData(0x19);
            SendData(0x03);
            SendData(0x0F);
            SendData(0x05);
            SendData(0x32);
            SendData(0x45);
            SendData(0x46);
            SendData(0x04);
            SendData(0x0E);
            SendData(0x0D);
            SendData(0x35);
            SendData(0x37);
            SendData(0x0F);

            SendCommand(0XC0); // Power Control 1
            SendData(0x17);
            SendData(0x15);

            SendCommand(0xC1); // Power Control 2
            SendData(0x41);

            SendCommand(0xC5); // VCOM Control
            SendData(0x00);
            SendData(0x12);
            SendData(0x80);

            SendCommand((byte)Register.MADCTL); // Memory Access Control
            SendData(0x48);          // MX, BGR

            SendCommand((byte)Register.COLOR_MODE); // Pixel Interface Format
            SendData(0x66); //24bpp

            SendCommand(0xB0); // Interface Mode Control
            SendData(0x00);

            SendCommand(0xB1); // Frame Rate Control
            SendData(0xA0); //60hz

            SendCommand(0xB4); // Display Inversion Control
            SendData(0x02); //2-dot

            SendCommand(0xB6); // Display Function Control
            SendData(0x02);
            SendData(0x02);
            SendData(0x3B);

            SendCommand(0xE9); //set image funcion 
            SendData(0x00); //disable 24 bit data

            SendCommand(0xF7); // Adjust Control 3
            SendData(0xA9);
            SendData(0x51);
            SendData(0x2C);
            SendData(0x82);// D7 stream, loose

            SendCommand(0xB7); // Entry Mode Set
            SendData(0xC6);

            SendCommand(Register.SLPOUT);  //Exit Sleep
            DelayMs(120);

            SendCommand(Register.DISPON);  //Display on
            DelayMs(25);
        }

        /// <summary>
        /// Set addrees window for display updates
        /// </summary>
        /// <param name="x0">X start in pixels</param>
        /// <param name="y0">Y start in pixels</param>
        /// <param name="x1">X end in pixels</param>
        /// <param name="y1">Y end in pixels</param>
        protected override void SetAddressWindow(int x0, int y0, int x1, int y1)
        {
            SendCommand(LcdCommand.CASET);  // column addr set
            dataCommandPort.State = Data;
            Write((byte)(x0 >> 8));
            Write((byte)(x0 & 0xff));   // XSTART 
            Write((byte)(x1 >> 8));
            Write((byte)(x1 & 0xff));   // XEND

            SendCommand(LcdCommand.RASET);  // row addr set
            dataCommandPort.State = Data;
            Write((byte)(y0 >> 8));
            Write((byte)(y0 & 0xff));    // YSTART
            Write((byte)(y1 >> 8));
            Write((byte)(y1 & 0xff));    // YEND

            SendCommand(LcdCommand.RAMWR);  // write to RAM
        }

        /// <summary>
        /// Set the display rotation
        /// </summary>
        /// <param name="rotation">The rotation value</param>
        public void SetRotation(RotationType rotation)
        {
            SendCommand((byte)Register.MADCTL);

            switch (rotation)
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
        }
    }
}