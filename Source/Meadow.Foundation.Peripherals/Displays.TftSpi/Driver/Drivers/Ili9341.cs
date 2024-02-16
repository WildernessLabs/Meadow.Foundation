using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Units;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a Ili9341 TFT color display
    /// </summary>
    public class Ili9341 : TftSpiBase, IRotatableDisplay
    {
        /// <summary>
        /// SPI bus speed
        /// </summary>
        public override Frequency DefaultSpiBusSpeed => new Frequency(24000, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// The default display color mode
        /// </summary>
        public override ColorMode DefaultColorMode => ColorMode.Format12bppRgb444;

        /// <summary>
        /// The color modes supported by the display
        /// </summary>
        public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565 | ColorMode.Format12bppRgb444;

        /// <summary>
        /// Create a new Ili9341 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public Ili9341(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width, int height, ColorMode colorMode = ColorMode.Format12bppRgb444)
            : base(spiBus, chipSelectPin, dcPin, resetPin, width, height, colorMode)
        {
            Initialize();
        }

        /// <summary>
        /// Create a new Ili9341 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public Ili9341(ISpiBus spiBus, IDigitalOutputPort chipSelectPort,
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
                DelayMs(120); //Not sure if this is needed but can't hurt
            }

            SendCommand(0xEF, new byte[] { 0x03, 0x80, 0x02 });
            SendCommand(0xCF, new byte[] { 0x00, 0xC1, 0x30 });
            SendCommand(0xED, new byte[] { 0x64, 0x03, 0x12, 0x81 });
            SendCommand(0xE8, new byte[] { 0x85, 0x00, 0x78 });

            SendCommand(0xCB, new byte[] { 0x39, 0x2C, 0x00, 0x34, 0x02 });
            SendCommand(0xF7, new byte[] { 0x20 });
            SendCommand(0xEA, new byte[] { 0x00, 0x00 });
            SendCommand(ILI9341_PWCTR1, new byte[] { 0x23 });
            SendCommand(ILI9341_PWCTR2, new byte[] { 0x10 });
            SendCommand(ILI9341_VMCTR1, new byte[] { 0x3E, 0x28 });
            SendCommand(ILI9341_VMCTR2, new byte[] { 0x86 });
            SendCommand((byte)Register.MADCTL, new byte[] { (byte)(Register.MADCTL_MX | Register.MADCTL_BGR) });

            if (ColorMode == ColorMode.Format16bppRgb565)
            {
                SendCommand((byte)Register.COLOR_MODE, new byte[] { 0x55 }); //color mode - 16bpp  
            }
            else
            {
                SendCommand((byte)Register.COLOR_MODE, new byte[] { 0x53 }); //color mode - 12bpp 
            }
            SendCommand((byte)Register.FRMCTR1, new byte[] { 0x00, 0x18 });
            SendCommand(ILI9341_DFUNCTR, new byte[] { 0x08, 0x82, 0x27 });
            SendCommand(0xF2, new byte[] { 0x00 });
            SendCommand(ILI9341_GAMMASET, new byte[] { 0x01 });
            SendCommand(ILI9341_GMCTRP1, new byte[] { 0x0F, 0x31, 0x2B, 0x0C, 0x0E, 0x08, 0x4E, 0xF1, 0x37, 0x07, 0x10, 0x03, 0x0E, 0x09, 0x00 });
            SendCommand(ILI9341_GMCTRN1, new byte[] { 0x00, 0x0E, 0x14, 0x03, 0x11, 0x07, 0x31, 0xC1, 0x48, 0x08, 0x0F, 0x0C, 0x31, 0x36, 0x0F });
            SendCommand(Register.SLPOUT);
            DelayMs(120);
            SendCommand(Register.DISPON);

            dataCommandPort.State = Data;
        }

        void SendCommand(byte command, byte[] data)
        {
            dataCommandPort.State = Command;
            Write(command);

            if (data != null)
            {
                dataCommandPort.State = Data;
                Write(data);
            }
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
                    SendData((byte)Register.MADCTL_MY | (byte)Register.MADCTL_BGR);
                    break;
                case RotationType._270Degrees:
                    SendData((byte)Register.MADCTL_MX | (byte)Register.MADCTL_MY | (byte)Register.MADCTL_MV | (byte)Register.MADCTL_BGR);
                    break;
            }

            UpdateBuffer();
        }

        static readonly byte ILI9341_GAMMASET = 0x26;

        static readonly byte ILI9341_DFUNCTR = 0xB6;

        static readonly byte ILI9341_PWCTR1 = 0xC0;
        static readonly byte ILI9341_PWCTR2 = 0xC1;
        static readonly byte ILI9341_VMCTR1 = 0xC5;
        static readonly byte ILI9341_VMCTR2 = 0xC7;

        static readonly byte ILI9341_GMCTRP1 = 0xE0;
        static readonly byte ILI9341_GMCTRN1 = 0xE1;
    }
}