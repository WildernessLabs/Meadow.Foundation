using System;
using Meadow.Devices;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Ssd130x
{
    /// <summary>
    /// Provide an interface to the SSD1306 family of OLED displays.
    /// </summary>
    public partial class Ssd1306 : Ssd130xBase
    {
        /// <summary>
        ///     Create a new SSD1306 object using the default parameters for
        /// </summary>
        /// <remarks>
        ///     Note that by default, any pixels out of bounds will throw and exception.
        ///     This can be changed by setting the <seealso cref="IgnoreOutOfBoundsPixels" />
        ///     property to true.
        /// </remarks>
        /// <param name="displayType">Type of SSD1306 display (default = 128x64 pixel display).</param>
        ///
        public Ssd1306(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
            DisplayType displayType = DisplayType.OLED128x64)
        {
            dataCommandPort = device.CreateDigitalOutputPort(dcPin, false);
            resetPort = device.CreateDigitalOutputPort(resetPin, true);
            chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin, false);

            spi = spiBus;
            spiPeripheral = new SpiPeripheral(spiBus, chipSelectPort);

            connectionType = ConnectionType.SPI;

            InitSSD1306(displayType);
        }

        /// <summary>
        ///     Create a new SSD1306 object using the default parameters for
        /// </summary>
        /// <remarks>
        ///     Note that by default, any pixels out of bounds will throw and exception.
        ///     This can be changed by setting the <seealso cref="IgnoreOutOfBoundsPixels" />
        ///     property to true.
        /// </remarks>
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
            switch (displayType)
            {
                case DisplayType.OLED128x64:
                    width = 128;
                    height = 64;
                    SendCommands(oled128x64SetupSequence);
                    break;
                case DisplayType.OLED64x48:
                    width = 128;
                    height = 64;
                    xOffset = 32;
                    yOffset = 16;
                    SendCommands(oled128x64SetupSequence);
                    break;
                case DisplayType.OLED72x40:
                    width = 72;
                    height = 40;
                    SendCommands(oled72x40SetupSequence);
                    break;
                case DisplayType.OLED128x32:
                    width = 128;
                    height = 32;
                    SendCommands(oled128x32SetupSequence);
                    break;
                case DisplayType.OLED96x16:
                    width = 96;
                    height = 16;
                    SendCommands(oled96x16SetupSequence);
                    break;
            }

            //align buffer to PAGE_SIZE
            int bufferSize = width * height / 8;
            bufferSize += bufferSize % 16;

            buffer = new byte[bufferSize];

            if (connectionType == ConnectionType.SPI)
            {
                spiReceive = new byte[width * height / 8];
            }

            showPreamble = new byte[] { 0x21, 0x00, (byte)(width - 1), 0x22, 0x00, (byte)(height / 8 - 1) };

            IgnoreOutOfBoundsPixels = false;

            //
            //  Finally, put the display into a known state.
            //
            InvertDisplay = false;
            Sleep = false;
            Contrast = 0xff;
            StopScrolling();
        }
    }
}