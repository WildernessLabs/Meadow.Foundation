using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a Gc9a01 TFT color display
    /// </summary>
    public class Gc9a01 : TftSpiBase, IRotatableDisplay
    {
        /// <summary>
        /// The display default color mode
        /// </summary>
        public override ColorMode DefaultColorMode => ColorMode.Format16bppRgb565;

        /// <summary>
        /// The color modes supported by the display
        /// </summary>
        public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565;

        /// <summary>
        /// Create a new Gc9a01 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        public Gc9a01(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin) :
            base(spiBus, chipSelectPin, dcPin, resetPin, 240, 240, ColorMode.Format16bppRgb565)
        {
            Initialize();

            SetRotation(RotationType.Normal);
        }

        /// <summary>
        /// Create a new Gc9a01 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        public Gc9a01(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, 240, 240, ColorMode.Format16bppRgb565)
        {
            Initialize();

            SetRotation(RotationType.Normal);
        }

        /// <summary>
        /// Initialize the display
        /// </summary>
        protected override void Initialize()
        {
            if (resetPort != null)
            {
                resetPort.State = false;
                DelayMs(10);
                resetPort.State = true;
                DelayMs(120);
            }

            SendCommand(0xEF);
            SendCommand(0xEB);
            SendData(0x14);

            SendCommand(0xFE);
            SendCommand(0xEF);

            SendCommand(0xEB);
            SendData(0x14);

            SendCommand(0x84);
            SendData(0x40);

            SendCommand(0x85);
            SendData(0xFF);

            SendCommand(0x86);
            SendData(0xFF);

            SendCommand(0x87);
            SendData(0xFF);

            SendCommand(0x88);
            SendData(0x0A);

            SendCommand(0x89);
            SendData(0x21);

            SendCommand(0x8A);
            SendData(0x00);

            SendCommand(0x8B);
            SendData(0x80);

            SendCommand(0x8C);
            SendData(0x01);

            SendCommand(0x8D);
            SendData(0x01);

            SendCommand(0x8E);
            SendData(0xFF);

            SendCommand(0x8F);
            SendData(0xFF);

            SendCommand(0xB6);
            SendData(0x00);
            SendData(0x20); //0x00

            SendCommand(0x3A);
            SendData(0x05);

            SendCommand(0x90);
            SendData(0x08);
            SendData(0x08);
            SendData(0x08);
            SendData(0x08);

            SendCommand(0xBD);
            SendData(0x06);

            SendCommand(0xBC);
            SendData(0x00);

            SendCommand(0xFF);
            SendData(0x60);
            SendData(0x01);
            SendData(0x04);

            SendCommand(0xC3);
            SendData(0x13);
            SendCommand(0xC4);
            SendData(0x13);

            SendCommand(0xC9);
            SendData(0x22);

            SendCommand(0xBE);
            SendData(0x11);

            SendCommand(0xE1);
            SendData(0x10);
            SendData(0x0E);

            SendCommand(0xDF);
            SendData(0x21);
            SendData(0x0C);
            SendData(0x02);

            SendCommand(0xF0);
            SendData(0x45);
            SendData(0x09);
            SendData(0x08);
            SendData(0x08);
            SendData(0x26);
            SendData(0x2A);

            SendCommand(0xF1);
            SendData(0x43);
            SendData(0x70);
            SendData(0x72);
            SendData(0x36);
            SendData(0x37);
            SendData(0x6F);

            SendCommand(0xF2);
            SendData(0x45);
            SendData(0x09);
            SendData(0x08);
            SendData(0x08);
            SendData(0x26);
            SendData(0x2A);

            SendCommand(0xF3);
            SendData(0x43);
            SendData(0x70);
            SendData(0x72);
            SendData(0x36);
            SendData(0x37);
            SendData(0x6F);

            SendCommand(0xED);
            SendData(0x1B);
            SendData(0x0B);

            SendCommand(0xAE);
            SendData(0x77);

            SendCommand(0xCD);
            SendData(0x63);

            SendCommand(0x70);
            SendData(0x07);
            SendData(0x07);
            SendData(0x04);
            SendData(0x0E);
            SendData(0x0F);
            SendData(0x09);
            SendData(0x07);
            SendData(0x08);
            SendData(0x03);

            SendCommand(0xE8);
            SendData(0x34);

            SendCommand(0x62);
            SendData(0x18);
            SendData(0x0D);
            SendData(0x71);
            SendData(0xED);
            SendData(0x70);
            SendData(0x70);
            SendData(0x18);
            SendData(0x0F);
            SendData(0x71);
            SendData(0xEF);
            SendData(0x70);
            SendData(0x70);

            SendCommand(0x63);
            SendData(0x18);
            SendData(0x11);
            SendData(0x71);
            SendData(0xF1);
            SendData(0x70);
            SendData(0x70);
            SendData(0x18);
            SendData(0x13);
            SendData(0x71);
            SendData(0xF3);
            SendData(0x70);
            SendData(0x70);

            SendCommand(0x64);
            SendData(0x28);
            SendData(0x29);
            SendData(0xF1);
            SendData(0x01);
            SendData(0xF1);
            SendData(0x00);
            SendData(0x07);

            SendCommand(0x66);
            SendData(0x3C);
            SendData(0x00);
            SendData(0xCD);
            SendData(0x67);
            SendData(0x45);
            SendData(0x45);
            SendData(0x10);
            SendData(0x00);
            SendData(0x00);
            SendData(0x00);

            SendCommand(0x67);
            SendData(0x00);
            SendData(0x3C);
            SendData(0x00);
            SendData(0x00);
            SendData(0x00);
            SendData(0x01);
            SendData(0x54);
            SendData(0x10);
            SendData(0x32);
            SendData(0x98);

            SendCommand(0x74);
            SendData(0x10);
            SendData(0x85);
            SendData(0x80);
            SendData(0x00);
            SendData(0x00);
            SendData(0x4E);
            SendData(0x00);

            SendCommand(0x98);
            SendData(0x3e);
            SendData(0x07);

            SendCommand(0x35);

            SendCommand(Register.INVON);

            SendCommand(Register.SLPOUT);
            DelayMs(120);
            SendCommand(Register.DISPON);
            DelayMs(20);
        }

        /// <summary>
        /// Set the rotation of the display
        /// </summary>
        /// <param name="rotation">The rotation</param>
        public void SetRotation(RotationType rotation)
        {
            SendCommand(Register.MADCTL);

            switch (Rotation = rotation)
            {
                case RotationType.Normal:
                    SendData((byte)(Register.MADCTL_MX | Register.MADCTL_MY | Register.MADCTL_BGR));
                    break;
                case RotationType._90Degrees:
                    SendData((byte)(Register.MADCTL_MY | Register.MADCTL_MV | Register.MADCTL_BGR));
                    break;
                case RotationType._180Degrees:
                    SendData((byte)Register.MADCTL_BGR);
                    break;
                case RotationType._270Degrees:
                    SendData((byte)(Register.MADCTL_MX | Register.MADCTL_MV | Register.MADCTL_BGR));
                    break;
            }
        }
    }
}