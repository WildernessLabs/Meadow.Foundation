using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Hardware;

namespace Meadow.Foundation.mikroBUS.Sensors.Atmospheric
{
    /// <summary>
    /// Represents a mikroBUS Thermo 13 Click board
    /// </summary>
    public class CThermo13 : Bh1900Nux
    {
        /// <summary>
        /// Creates a CThermo13 driver
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        public CThermo13(II2cBus i2cBus)
            : base(i2cBus, Addresses.Default)
        { }

        /// <summary>
        /// Creates a CThermo13 driver using a MikroBus connector
        /// </summary>
        /// <param name="connector">The MikroBus connector</param>
        public CThermo13(MikroBusConnector connector)
            : base(connector.I2cBus, Addresses.Default)
        { }
    }
}