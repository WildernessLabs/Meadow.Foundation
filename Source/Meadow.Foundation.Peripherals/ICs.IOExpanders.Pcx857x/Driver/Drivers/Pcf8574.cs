using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents the Pcf8574 8-bit I/O I2C expander
    /// </summary>
    public class Pcf8574 : Pcx8574
    {
        /// <summary>
        /// Initializes a new instance of the Pcf8574 device
        /// </summary>
        /// <param name="i2cBus">The I2C bus the peripheral is connected to</param>
        /// <param name="address">The I2C bus address of the peripheral</param>
        /// <param name="interruptPin">The interrupt pin</param>
        public Pcf8574(II2cBus i2cBus, byte address, IPin? interruptPin)
            : base(i2cBus, address, interruptPin)
        { }

        /// <summary>
        /// Initializes a new instance of the Pcf8574 device
        /// </summary>
        /// <param name="i2cBus">The I2C bus the peripheral is connected to</param>
        /// <param name="address">The I2C bus address of the peripheral</param>
        /// <param name="interruptPort">The interrupt port</param>
        public Pcf8574(II2cBus i2cBus, byte address, IDigitalInterruptPort? interruptPort = default)
            : base(i2cBus, address, interruptPort)
        { }
    }
}