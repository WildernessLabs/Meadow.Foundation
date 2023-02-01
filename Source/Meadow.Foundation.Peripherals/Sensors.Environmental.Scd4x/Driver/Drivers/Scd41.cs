using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents an SCD41 C02 sensor
    /// </summary>
    public class Scd41 : Scd4xBase
    {
        /// <summary>
        /// Create a new Scd41 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public Scd41(II2cBus i2cBus, byte address = 98)
            : base(i2cBus, address)
        { }
    }
}
