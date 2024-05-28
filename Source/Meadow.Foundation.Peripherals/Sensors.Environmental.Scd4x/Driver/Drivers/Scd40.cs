using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents an SCD40 C02 sensor
    /// </summary>
    public class Scd40 : Scd4xBase
    {
        /// <summary>
        /// Create a new Scd40 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public Scd40(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address)
        { }
    }
}