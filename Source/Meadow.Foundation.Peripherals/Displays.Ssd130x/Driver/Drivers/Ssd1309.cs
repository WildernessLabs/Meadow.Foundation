using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Provide an interface to the SSD1309 family of OLED displays
    /// </summary>
    public class Ssd1309 : Ssd1306
    {
        /// <summary>
        /// The default SPI clock mode
        /// </summary>
        public static SpiClockConfiguration.Mode DefaultSpiClockMode = SpiClockConfiguration.Mode.Mode0;

        /// <summary>
        /// Default SPI frequency
        /// </summary>
        public static Frequency DefaultSpiBusSpeed = new Frequency(12000, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// Create a new Ssd1309 object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        public Ssd1309(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin) :
            base(spiBus, chipSelectPin, dcPin, resetPin, DisplayType.OLED128x64)
        {
        }

        /// <summary>
        /// Create a new Ssd1309 display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        public Ssd1309(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, IDigitalOutputPort dataCommandPort, IDigitalOutputPort resetPort) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort)
        {
        }

        /// <summary>
        /// Create a new SSD1309 object
        /// </summary>
        /// <param name="i2cBus">I2cBus connected to display</param>
        /// <param name="address">Address of the bus on the I2C display.</param>
        public Ssd1309(II2cBus i2cBus, byte address = (byte)Addresses.Default) : 
            base(i2cBus, address, DisplayType.OLED128x64)
        {
        }
    }
}