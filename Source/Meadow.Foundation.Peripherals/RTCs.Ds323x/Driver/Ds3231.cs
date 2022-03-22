using Meadow.Hardware;

namespace Meadow.Foundation.RTCs
{
    /// <summary>
    /// Create a new DS3231 Real Time Clock object.
    /// </summary>
    public partial class Ds3231 : Ds323x
    {
        /// <summary>
        /// Create a new MAG3110 object using the default parameters for the component.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="interruptPin">Digital pin connected to the alarm interrupt pin on the RTC.</param>
        /// <param name="i2cBus">The I2C Bus the peripheral is connected to</param>
        /// <param name="address">I2C Bus address of the peripheral</param>
        public Ds3231(
            IDigitalInputController device,
            II2cBus i2cBus,
            IPin interruptPin = null,
            byte address = (byte)Address.Default)
            : base(new I2cPeripheral(i2cBus, address), device, interruptPin)
        {
        }

        /// <summary>
        /// Create a new Ds3231 object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the DS3231 (default = 0x68).</param>
        /// <param name="speed">Speed of the I2C bus (default = 100 KHz).</param>
        /// <param name="i2cBus">The I2C Bus the peripheral is connected to</param>
        /// <param name="interruptPort">Digital port connected to the alarm interrupt pin on the RTC.</param>
        public Ds3231(
           II2cBus i2cBus,
           IDigitalInputPort interruptPort = null,
           byte address = (byte)Address.Default)
           : base(new I2cPeripheral(i2cBus, address), interruptPort)
        {
        }
    }
}