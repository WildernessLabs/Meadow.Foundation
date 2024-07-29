using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Units;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a Ili9225 TFT color display
    /// </summary>
    public class Ili9225 : TftSpiBase, IRotatableDisplay
    {
        /// <summary>
        /// SPI bus speed
        /// </summary>
        public override Frequency DefaultSpiBusSpeed => new(6000, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// The default display color mode
        /// </summary>
        public override ColorMode DefaultColorMode => ColorMode.Format16bppRgb565;

        /// <summary>
        /// The color modes supported by the display
        /// </summary>
        public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565 | ColorMode.Format12bppRgb444 | ColorMode.Format18bppRgb666;

        /// <summary>
        /// Create a new Ili9225 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public Ili9225(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            ColorMode colorMode = ColorMode.Format16bppRgb565)
            : base(spiBus, chipSelectPin, dcPin, resetPin, 176, 220, colorMode)
        {
            Initialize();
            SetRotation(RotationType.Default);
        }

        /// <summary>
        /// Create a new Ili9225 color display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="colorMode">The color mode to use for the display buffer</param>
        public Ili9225(ISpiBus spiBus, IDigitalOutputPort chipSelectPort,
                IDigitalOutputPort dataCommandPort, IDigitalOutputPort resetPort,
                ColorMode colorMode = ColorMode.Format16bppRgb565) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, 176, 220, colorMode)
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

            SendCommand(ILI9225_POWER_CTRL1);
            SendData(0x00); SendData(0x00);
            SendCommand(ILI9225_POWER_CTRL2);
            SendData(0x00); SendData(0x00);
            SendCommand(ILI9225_POWER_CTRL3);
            SendData(0x00); SendData(0x00);
            SendCommand(ILI9225_POWER_CTRL4);
            SendData(0x00); SendData(0x00);
            SendCommand(ILI9225_POWER_CTRL5);
            SendData(0x00); SendData(0x00);

            Thread.Sleep(40);

            SendCommand(ILI9225_POWER_CTRL2);
            SendData(0x00); SendData(0x18);
            SendCommand(ILI9225_POWER_CTRL3);
            SendData(0x61); SendData(0x21);
            SendCommand(ILI9225_POWER_CTRL4);
            SendData(0x00); SendData(0x6F);
            SendCommand(ILI9225_POWER_CTRL5);
            SendData(0x49); SendData(0x5F);
            SendCommand(ILI9225_POWER_CTRL1);
            SendData(0x08); SendData(0x00);

            Thread.Sleep(10);

            SendCommand(ILI9225_POWER_CTRL2);
            SendData(0x10); SendData(0x3B);

            Thread.Sleep(50);

            SendCommand(ILI9225_LCD_AC_DRIVING_CTRL);
            SendData(0x01); SendData(0x00);
            SendCommand(ILI9225_DISP_CTRL1);
            SendData(0x00); SendData(0x00);
            SendCommand(ILI9225_BLANK_PERIOD_CTRL1);
            SendData(0x08); SendData(0x08);
            SendCommand(ILI9225_FRAME_CYCLE_CTRL);
            SendData(0x11); SendData(0x00);
            SendCommand(ILI9225_INTERFACE_CTRL);
            SendData(0x00); SendData(0x00);
            SendCommand(ILI9225_OSC_CTRL);
            SendData(0x0D); SendData(0x01);
            SendCommand(ILI9225_VCI_RECYCLING);
            SendData(0x00); SendData(0x20);
            SendCommand(ILI9225_RAM_ADDR_SET1);
            SendData(0x00); SendData(0x00);
            SendCommand(ILI9225_RAM_ADDR_SET2);
            SendData(0x00); SendData(0x00);

            SendCommand(ILI9225_GATE_SCAN_CTRL);
            SendData(0x00); SendData(0x00);
            SendCommand(ILI9225_VERTICAL_SCROLL_CTRL1);
            SendData(0x00); SendData(0xDB);
            SendCommand(ILI9225_VERTICAL_SCROLL_CTRL2);
            SendData(0x00); SendData(0x00);
            SendCommand(ILI9225_VERTICAL_SCROLL_CTRL3);
            SendData(0x00); SendData(0x00);
            SendCommand(ILI9225_PARTIAL_DRIVING_POS1);
            SendData(0x00); SendData(0xDB);
            SendCommand(ILI9225_PARTIAL_DRIVING_POS2);
            SendData(0x00); SendData(0x00);
            SendCommand(ILI9225_HORIZONTAL_WINDOW_ADDR1);
            SendData(0x00); SendData(0xAF);
            SendCommand(ILI9225_HORIZONTAL_WINDOW_ADDR2);
            SendData(0x00); SendData(0x00);
            SendCommand(ILI9225_VERTICAL_WINDOW_ADDR1);
            SendData(0x00); SendData(0xDB);
            SendCommand(ILI9225_VERTICAL_WINDOW_ADDR2);
            SendData(0x00); SendData(0x00);

            /* Set GAMMA curve */
            SendCommand(ILI9225_GAMMA_CTRL1);
            SendData(0x00); SendData(0x00);
            SendCommand(ILI9225_GAMMA_CTRL2);
            SendData(0x08); SendData(0x08);
            SendCommand(ILI9225_GAMMA_CTRL3);
            SendData(0x08); SendData(0x0A);
            SendCommand(ILI9225_GAMMA_CTRL4);
            SendData(0x00); SendData(0x0A);
            SendCommand(ILI9225_GAMMA_CTRL5);
            SendData(0x0A); SendData(0x08);
            SendCommand(ILI9225_GAMMA_CTRL6);
            SendData(0x08); SendData(0x08);
            SendCommand(ILI9225_GAMMA_CTRL7);
            SendData(0x00); SendData(0x00);
            SendCommand(ILI9225_GAMMA_CTRL8);
            SendData(0x0A); SendData(0x00);
            SendCommand(ILI9225_GAMMA_CTRL9);
            SendData(0x07); SendData(0x10);
            SendCommand(ILI9225_GAMMA_CTRL10);
            SendData(0x07); SendData(0x10);

            SendCommand(ILI9225_DISP_CTRL1);
            SendData(0x00); SendData(0x12);

            Thread.Sleep(50);

            SendCommand(ILI9225_DISP_CTRL1);
            SendData(0x10); SendData(0x17);
        }

        /// <summary>
        /// Set the display rotation
        /// </summary>
        /// <param name="rotation">The rotation value</param>
        public void SetRotation(RotationType rotation)
        {
            SendCommand(ILI9225_DRIVER_OUTPUT_CTRL);
            switch (Rotation = rotation)
            {
                case RotationType.Normal:
                    SendData(0x01);
                    SendData(0x1C);
                    SendCommand(ILI9225_ENTRY_MODE);
                    SendData(0x10);
                    SendData(0x30);
                    break;
                case RotationType._90Degrees:
                    SendData(0x00);
                    SendData(0x1C);
                    SendCommand(ILI9225_ENTRY_MODE);
                    SendData(0x10);
                    SendData(0x38);
                    break;
                case RotationType._180Degrees:
                    SendData(0x02);
                    SendData(0x1C);
                    SendCommand(ILI9225_ENTRY_MODE);
                    SendData(0x10);
                    SendData(0x30);
                    break;
                case RotationType._270Degrees:
                    SendData(0x03);
                    SendData(0x1C);
                    SendCommand(ILI9225_ENTRY_MODE);
                    SendData(0x10);
                    SendData(0x38);
                    break;
            }


            UpdateBuffer();
        }

        byte ILI9225_DRIVER_OUTPUT_CTRL = 0x01;  // Driver Output Control
        byte ILI9225_LCD_AC_DRIVING_CTRL = 0x02;  // LCD AC Driving Control
        byte ILI9225_ENTRY_MODE = 0x03;  // Entry Mode
        byte ILI9225_DISP_CTRL1 = 0x07;  // Display Control 1
        byte ILI9225_BLANK_PERIOD_CTRL1 = 0x08;  // Blank Period Control
        byte ILI9225_FRAME_CYCLE_CTRL = 0x0B;  // Frame Cycle Control
        byte ILI9225_INTERFACE_CTRL = 0x0C;  // Interface Control
        byte ILI9225_OSC_CTRL = 0x0F;  // Osc Control
        byte ILI9225_POWER_CTRL1 = 0x10;  // Power Control 1
        byte ILI9225_POWER_CTRL2 = 0x11;  // Power Control 2
        byte ILI9225_POWER_CTRL3 = 0x12;  // Power Control 3
        byte ILI9225_POWER_CTRL4 = 0x13;  // Power Control 4
        byte ILI9225_POWER_CTRL5 = 0x14;  // Power Control 5
        byte ILI9225_VCI_RECYCLING = 0x15;  // VCI Recycling
        byte ILI9225_RAM_ADDR_SET1 = 0x20;  // Horizontal GRAM Address Set
        byte ILI9225_RAM_ADDR_SET2 = 0x21;  // Vertical GRAM Address Set
        byte ILI9225_GRAM_DATA_REG = 0x22;  // GRAM Data Register
        byte ILI9225_GATE_SCAN_CTRL = 0x30;  // Gate Scan Control Register
        byte ILI9225_VERTICAL_SCROLL_CTRL1 = 0x31;  // Vertical Scroll Control 1 Register
        byte ILI9225_VERTICAL_SCROLL_CTRL2 = 0x32;  // Vertical Scroll Control 2 Register
        byte ILI9225_VERTICAL_SCROLL_CTRL3 = 0x33;  // Vertical Scroll Control 3 Register
        byte ILI9225_PARTIAL_DRIVING_POS1 = 0x34;  // Partial Driving Position 1 Register
        byte ILI9225_PARTIAL_DRIVING_POS2 = 0x35;  // Partial Driving Position 2 Register
        byte ILI9225_HORIZONTAL_WINDOW_ADDR1 = 0x36;  // Horizontal Address Start Position
        byte ILI9225_HORIZONTAL_WINDOW_ADDR2 = 0x37;  // Horizontal Address End Position
        byte ILI9225_VERTICAL_WINDOW_ADDR1 = 0x38;  // Vertical Address Start Position
        byte ILI9225_VERTICAL_WINDOW_ADDR2 = 0x39;  // Vertical Address End Position
        byte ILI9225_GAMMA_CTRL1 = 0x50;  // Gamma Control 1
        byte ILI9225_GAMMA_CTRL2 = 0x51;  // Gamma Control 2
        byte ILI9225_GAMMA_CTRL3 = 0x52;  // Gamma Control 3
        byte ILI9225_GAMMA_CTRL4 = 0x53;  // Gamma Control 4
        byte ILI9225_GAMMA_CTRL5 = 0x54;  // Gamma Control 5
        byte ILI9225_GAMMA_CTRL6 = 0x55;  // Gamma Control 6
        byte ILI9225_GAMMA_CTRL7 = 0x56;  // Gamma Control 7
        byte ILI9225_GAMMA_CTRL8 = 0x57;  // Gamma Control 8
        byte ILI9225_GAMMA_CTRL9 = 0x58;  // Gamma Control 9
        byte ILI9225_GAMMA_CTRL10 = 0x59;  // Gamma Control 10
    }
}