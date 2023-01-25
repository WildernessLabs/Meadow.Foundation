using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using Meadow.Units;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a St7789 TFT color display
    /// </summary>
    public class St7789 : TftSpiBase
    {
        /// <summary>
        /// The default SPI bus frequency
        /// </summary>
        public static Frequency DefaultSpiBusSpeed = new Frequency(48000, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// The default display color mode
        /// </summary>
        public override ColorType DefautColorMode => ColorType.Format16bppRgb565;

        private byte rowStart, rowStart2;
        private byte columnStart, columnStart2;

        private byte xOffset = 0;
        private byte yOffset = 0;

        /// <summary>
        /// Create a new St7789 color display object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public St7789(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            int width, int height, ColorType colorMode = ColorType.Format12bppRgb444) 
            : base(device, spiBus, chipSelectPin, dcPin, resetPin, width, height, colorMode)
        {
            Initialize();
        }

        /// <summary>
        /// Create a new St7789 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public St7789(ISpiBus spiBus, IDigitalOutputPort chipSelectPort,
                IDigitalOutputPort dataCommandPort, IDigitalOutputPort resetPort,
                int width, int height, ColorType colorMode = ColorType.Format12bppRgb444) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, width, height, colorMode)
        {
            Initialize();
        }

        /// <summary>
        /// Initalize the display
        /// </summary>
        protected override void Initialize()
        {
            if (resetPort != null)
            {
                resetPort.State = true;
                Thread.Sleep(50);
                resetPort.State = false;
                Thread.Sleep(50);
                resetPort.State = true;
                Thread.Sleep(50);
            }
            else
            {
                Thread.Sleep(150); //Not sure if this is needed but can't hurt
            }

            if (Width == 135) //135x240 
            {   //unknown if this is consistant across all displays with this res
                rowStart = rowStart2 = 40;
                columnStart = 53;
                columnStart2 = 52; 
            }
            else if(Width == 240 && Height == 240)
            {
                rowStart = 80;
                rowStart2 = columnStart = columnStart2 = 0;
            }
            else //1.47", 1.59", 1.9", 2.0" .... 320x240 etc.
            {
                rowStart = rowStart2 = (byte)((320 - Width) / 2);
                columnStart = columnStart2 = (byte)((240 - Width) / 2);
            }
            
            SendCommand(Register.SWRESET);
            DelayMs(150);
            SendCommand(Register.SLPOUT);
            DelayMs(500);

            SendCommand(Register.COLOR_MODE);  // set color mode - 16 bit color (x55), 12 bit color (x53), 18 bit color (x56)
            if (ColorMode == ColorType.Format16bppRgb565)
                SendData(0x55);  // 16-bit color RGB565
            else
                SendData(0x53); //12-bit color RGB444
           
            DelayMs(10);

            SendCommand(Register.MADCTL);
            SendData(0x00); //some variants use 0x08

            SendCommand((byte)LcdCommand.CASET);

            SendData(new byte[] { 0, 0, 0, (byte)Width });

            SendCommand((byte)LcdCommand.RASET);
            SendData(new byte[] { 0, 0, (byte)(Height >> 8), (byte)(Height & 0xFF) });

            SendCommand(Register.INVON); //inversion on
            DelayMs(10);
            SendCommand(Register.NORON); //normal display
            DelayMs(10);
            SendCommand(Register.DISPON); //display on
            DelayMs(500);

            SetAddressWindow(0, 0, (Width - 1), (Height - 1));

            dataCommandPort.State = Data;
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
            x0 += xOffset;
            y0 += yOffset;

            x1 += xOffset;
            y1 += yOffset;

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
            SendCommand(Register.MADCTL);

            switch (rotation)
            {
                case RotationType.Normal:
                    SendData((byte)Register.MADCTL_MX | (byte)Register.MADCTL_MY | (byte)Register.MADCTL_RGB);
                    xOffset = columnStart;
                    yOffset = rowStart;
                    break;
                case RotationType._90Degrees:
                    SendData((byte)Register.MADCTL_MY | (byte)Register.MADCTL_MV | (byte)Register.MADCTL_RGB);
                    xOffset = rowStart;
                    yOffset = columnStart2;
                    break;
                case RotationType._180Degrees:
                    SendData((byte)Register.MADCTL_RGB);
                    xOffset = columnStart2;
                    yOffset = rowStart2;
                    break;
                case RotationType._270Degrees:
                    SendData((byte)Register.MADCTL_MX | (byte)Register.MADCTL_MV | (byte)Register.MADCTL_RGB);
                    xOffset = rowStart2;
                    yOffset = columnStart;
                    break;
            }
        }
    }
}