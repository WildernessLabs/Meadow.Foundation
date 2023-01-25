using System.Threading;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a Rm68140 TFT color display
    /// </summary>
    public class Rm68140 : TftSpiBase
    {
        /// <summary>
        /// The default display color mode
        /// </summary>
        public override ColorType DefautColorMode => ColorType.Format12bppRgb444;

        /// <summary>
        /// Create a new Rm68140 color display object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public Rm68140(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width = 320, int height = 480, ColorType colorMode = ColorType.Format12bppRgb444) 
            : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height, colorMode)
        {
            Initialize();

            SetRotation(RotationType.Normal);
        }

        /// <summary>
        /// Create a new Rm68140 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public Rm68140(ISpiBus spiBus, IDigitalOutputPort chipSelectPort,
                IDigitalOutputPort dataCommandPort, IDigitalOutputPort resetPort,
                int width = 320, int height = 480, ColorType colorMode = ColorType.Format12bppRgb444) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, width, height, colorMode)
        {
            Initialize();

            SetRotation(RotationType.Normal);
        }

        /// <summary>
        /// Initalize the display
        /// </summary>
        protected override void Initialize()
        {
            SendCommand(Register.SLPOUT);
            Thread.Sleep(20);

            SendCommand(0xD0);
            SendData(0x07);
            SendData(0x42);
            SendData(0x18);

            SendCommand(0xD1);
            SendData(0x00);
            SendData(0x07);
            SendData(0x10);

            SendCommand(0xD2);
            SendData(0x01);
            SendData(0x02);

            SendCommand(0xC0);
            SendData(0x10);
            SendData(0x3B);
            SendData(0x00);
            SendData(0x02);
            SendData(0x11);

            SendCommand(0xC5);
            SendData(0x03);

            SendCommand(0xC8);
            SendData(0x00);
            SendData(0x32);
            SendData(0x36);
            SendData(0x45);
            SendData(0x06);
            SendData(0x16);
            SendData(0x37);
            SendData(0x75);
            SendData(0x77);
            SendData(0x54);
            SendData(0x0C);
            SendData(0x00);

            SendCommand((byte)Register.MADCTL);
            SendData(0x0A);

            SendCommand((byte)Register.COLOR_MODE);
            if (ColorMode == ColorType.Format16bppRgb565)
                SendData(0x55); //16 bit RGB565
            else
                SendData(0x53); //12 bit RGB444

            SendCommand((byte)LcdCommand.CASET);
            SendData(0x00);
            SendData(0x00);
            SendData(0x01);
            SendData(0x3F);

            SendCommand((byte)LcdCommand.RASET);
            SendData(0x00);
            SendData(0x00);
            SendData(0x01);
            SendData(0xDF);

            Thread.Sleep(120);
            SendCommand(Register.DISPON);
            Thread.Sleep(25);
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
            SendCommand((byte)LcdCommand.CASET);  // column addr set
            dataCommandPort.State = Data;
            Write((byte)(x0 >> 8));
            Write((byte)(x0 & 0xff));   // XSTART 
            Write((byte)(x1 >> 8));
            Write((byte)(x1 & 0xff));   // XEND

            SendCommand((byte)LcdCommand.RASET);  // row addr set
            dataCommandPort.State = Data;
            Write((byte)(y0 >> 8));
            Write((byte)(y0 & 0xff));    // YSTART
            Write((byte)(y1 >> 8));
            Write((byte)(y1 & 0xff));    // YEND

            SendCommand((byte)LcdCommand.RAMWR);  // write to RAM
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
                    SendData((byte)Register.MADCTL_BGR);
                    SendCommand(0xB6);
                    SendData(0);
                    SendData(0x22);
                    SendData(0x3B);
                    break;
                case RotationType._90Degrees:
                    SendData((byte)Register.MADCTL_MV | (byte)Register.MADCTL_BGR);
                    SendCommand(0xB6);
                    SendData(0);
                    SendData(0x02);
                    SendData(0x3B);
                    break;
                case RotationType._180Degrees:
                    SendData((byte)Register.MADCTL_BGR);
                    SendCommand(0xB6);
                    SendData(0);
                    SendData(0x42);
                    SendData(0x3B);
                    break;
                case RotationType._270Degrees:
                    SendData((byte)Register.MADCTL_MV | (byte)Register.MADCTL_BGR);
                    SendCommand(0xB6);
                    SendData(0);
                    SendData(0x62);
                    SendData(0x3B);
                    break;
            }
        }
    }
}