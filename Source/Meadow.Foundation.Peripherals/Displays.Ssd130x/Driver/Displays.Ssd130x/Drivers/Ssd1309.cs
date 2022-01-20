using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Displays.Ssd130x
{
    public class Ssd1309 : Ssd1306
    {
        public static SpiClockConfiguration.Mode DefaultSpiClockMode = SpiClockConfiguration.Mode.Mode0;
        public static Frequency DefaultSpiBusSpeed = new Frequency(12000, Frequency.UnitType.Kilohertz);

        /// <summary>
        ///     Create a new SSD1309 object using the default parameters for
        /// </summary>
        /// <remarks>
        ///     Note that by default, any pixels out of bounds will throw and exception.
        ///     This can be changed by setting the <seealso cref="IgnoreOutOfBoundsPixels" />
        ///     property to true.
        /// </remarks>
        public Ssd1309(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, DisplayType.OLED128x64)
        {
        }

        /// <summary>
        ///     Create a new SSD1309 object
        /// </summary>
        /// <param name="i2cBus">I2cBus connected to display</param>
        /// <param name="address">Address of the bus on the I2C display.</param>
        public Ssd1309(II2cBus i2cBus, byte address = (byte)Addresses.Default) : base(i2cBus, address, DisplayType.OLED128x64)
        {
        }
    }
}