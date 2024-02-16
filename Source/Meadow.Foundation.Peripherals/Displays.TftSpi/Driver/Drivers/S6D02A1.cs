using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a Samsung S6D02A1 TFT color display
    /// </summary>
    public class S6D02A1 : TftSpiBase, IRotatableDisplay
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
        /// Create a new S6D02A1 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public S6D02A1(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width = 128, int height = 160, ColorMode colorMode = ColorMode.Format12bppRgb444)
            : base(spiBus, chipSelectPin, dcPin, resetPin, width, height, colorMode)
        {
            Initialize();
        }

        /// <summary>
        /// Create a new S6D02A1 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public S6D02A1(ISpiBus spiBus, IDigitalOutputPort chipSelectPort,
                IDigitalOutputPort dataCommandPort, IDigitalOutputPort resetPort,
                int width = 128, int height = 160, ColorMode colorMode = ColorMode.Format12bppRgb444) :
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

            SendCommand(0xf0, new byte[] { 0x5a, 0x5a });             // Excommand2
            SendCommand(0xfc, new byte[] { 0x5a, 0x5a });             // Excommand3
            SendCommand(0x26, new byte[] { 0x01 });                   // Gamma set
            SendCommand(0xfa, new byte[] { 0x02, 0x1f, 0x00, 0x10, 0x22, 0x30, 0x38, 0x3A, 0x3A, 0x3A, 0x3A, 0x3A, 0x3d, 0x02, 0x01 });   // Positive gamma control
            SendCommand(0xfb, new byte[] { 0x21, 0x00, 0x02, 0x04, 0x07, 0x0a, 0x0b, 0x0c, 0x0c, 0x16, 0x1e, 0x30, 0x3f, 0x01, 0x02 });   // Negative gamma control
            SendCommand(0xfd, new byte[] { 0x00, 0x00, 0x00, 0x17, 0x10, 0x00, 0x01, 0x01, 0x00, 0x1f, 0x1f });                           // Analog parameter control
            SendCommand(0xf4, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x3f, 0x3f, 0x07, 0x00, 0x3C, 0x36, 0x00, 0x3C, 0x36, 0x00 });   // Power control
            SendCommand(0xf5, new byte[] { 0x00, 0x70, 0x66, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x6d, 0x66, 0x06 });               // VCOM control
            SendCommand(0xf6, new byte[] { 0x02, 0x00, 0x3f, 0x00, 0x00, 0x00, 0x02, 0x00, 0x06, 0x01, 0x00 });                           // Source control
            SendCommand(0xf2, new byte[] { 0x00, 0x01, 0x03, 0x08, 0x08, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x04, 0x08, 0x08 });   //Display control
            SendCommand(0xf8, new byte[] { 0x11 });                   // Gate control
            SendCommand(0xf7, new byte[] { 0xc8, 0x20, 0x00, 0x00 });  // Interface control
            SendCommand(0xf3, new byte[] { 0x00, 0x00 });              // Power sequence control
            SendCommand(0x11, null);                 // Wake
            DelayMs(150);
            SendCommand(0xf3, new byte[] { 0x00, 0x01 });  // Power sequence control
            DelayMs(50);
            SendCommand(0xf3, new byte[] { 0x00, 0x03 });  // Power sequence control
            DelayMs(50);
            SendCommand(0xf3, new byte[] { 0x00, 0x07 });  // Power sequence control
            DelayMs(50);
            SendCommand(0xf3, new byte[] { 0x00, 0x0f });  // Power sequence control
            DelayMs(150);
            SendCommand(0xf4, new byte[] { 0x00, 0x04, 0x00, 0x00, 0x00, 0x3f, 0x3f, 0x07, 0x00, 0x3C, 0x36, 0x00, 0x3C, 0x36, 0x00 });    // Power control
            DelayMs(50);
            SendCommand(0xf3, new byte[] { 0x00, 0x1f });   // Power sequence control
            DelayMs(50);
            SendCommand(0xf3, new byte[] { 0x00, 0x7f });   // Power sequence control
            DelayMs(50);
            SendCommand(0xf3, new byte[] { 0x00, 0xff });   // Power sequence control
            DelayMs(50);
            SendCommand(0xfd, new byte[] { 0x00, 0x00, 0x00, 0x17, 0x10, 0x00, 0x00, 0x01, 0x00, 0x16, 0x16 });                           // Analog parameter control
            SendCommand(0xf4, new byte[] { 0x00, 0x09, 0x00, 0x00, 0x00, 0x3f, 0x3f, 0x07, 0x00, 0x3C, 0x36, 0x00, 0x3C, 0x36, 0x00 });   // Power control
            SendCommand(0x36, new byte[] { 0xC8 }); // Memory access data control
            SendCommand(0x35, new byte[] { 0x00 }); // Tearing effect line on

            SendCommand(Register.COLOR_MODE);
            if (ColorMode == ColorMode.Format16bppRgb565)
                SendData(0x05); //16 bit RGB565
            else
                SendData(0x33); //12 bit RGB444

            DelayMs(150);
            SendCommand(0x29, null);                // Display on
            SendCommand(0x2c, null);				// Memory write

            dataCommandPort.State = Data;
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
                    SendData((byte)Register.MADCTL_MX | (byte)Register.MADCTL_MY | (byte)Register.MADCTL_BGR);
                    break;
                case RotationType._90Degrees:
                    SendData((byte)Register.MADCTL_MY | (byte)Register.MADCTL_MV | (byte)Register.MADCTL_BGR);
                    break;
                case RotationType._180Degrees:
                    SendData((byte)Register.MADCTL_BGR);
                    break;
                case RotationType._270Degrees:
                    SendData((byte)Register.MADCTL_MX | (byte)Register.MADCTL_MV | (byte)Register.MADCTL_BGR);
                    break;
            }

            UpdateBuffer();
        }

        void SendCommand(byte command, byte[]? data)
        {
            dataCommandPort.State = Command;
            Write(command);

            if (data != null)
            {
                dataCommandPort.State = Data;
                for (int i = 0; i < data.Length; i++)
                {
                    Write(data[i]);
                }
            }
        }
    }
}