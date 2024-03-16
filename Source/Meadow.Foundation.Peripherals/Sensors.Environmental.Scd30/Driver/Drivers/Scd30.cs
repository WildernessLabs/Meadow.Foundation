using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents an SCD30 C02 sensor
    /// </summary>
    public class Scd30 : Scd30Base
    {
        /// <summary>
        /// Create a new Scd30 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public Scd30(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address)
        { }
    }
}
