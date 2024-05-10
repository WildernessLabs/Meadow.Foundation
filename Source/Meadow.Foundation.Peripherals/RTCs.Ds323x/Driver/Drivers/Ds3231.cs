using Meadow.Hardware;

namespace Meadow.Foundation.RTCs
{
    /// <summary>
    /// Represents a Ds3231 real-time clock
    /// </summary>
    public partial class Ds3231 : Ds323x
    {
        /// <summary>
        /// Create a new Ds3231 RTC object
        /// </summary>
        /// <param name="interruptPin">Digital pin connected to the alarm interrupt pin on the RTC.</param>
        /// <param name="i2cBus">The I2C Bus the peripheral is connected to</param>
        /// <param name="address">I2C Bus address of the peripheral</param>
        public Ds3231(II2cBus i2cBus, IPin? interruptPin = null, byte address = (byte)Addresses.Default)
            : base(new I2cCommunications(i2cBus, address), interruptPin)
        { }

        /// <summary>
        /// Create a new Ds3231 RTC object
        /// </summary>
        /// <param name="address">Address of the DS3231 (default = 0x68).</param>
        /// <param name="i2cBus">The I2C Bus the peripheral is connected to</param>
        /// <param name="interruptPort">Digital port connected to the alarm interrupt pin on the RTC.</param>
        public Ds3231(
           II2cBus i2cBus,
           IDigitalInterruptPort? interruptPort = null,
           byte address = (byte)Addresses.Default)
           : base(new I2cCommunications(i2cBus, address), interruptPort)
        { }
    }
}