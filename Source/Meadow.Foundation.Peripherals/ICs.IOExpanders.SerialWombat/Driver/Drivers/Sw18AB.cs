using Meadow.Hardware;
using Meadow.Logging;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents an SW18AB I2C SerialWombat IO expander
    /// </summary>
    public class Sw18AB : SerialWombatBase
    {
        /// <summary>
        /// Creates a new Serial Wombat object 
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the wombat</param>
        /// <param name="address">The I2C address</param>
        /// <param name="logger">Meadow logger (optional)</param>
        public Sw18AB(II2cBus i2cBus, Addresses address = SerialWombatBase.Addresses.Default, Logger? logger = null)
            : base(i2cBus, address, logger)
        { }
    }
}