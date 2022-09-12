using Meadow.Hardware;

namespace Meadow.Foundation.ICs.FanControllers
{
    /// <summary>
    /// Repreents an EMC2101 fan controller and temperature monitor
    /// </summary>
    public partial class Emc2101
    {
        /// <summary>
        /// Communication bus used to communicate with the Emc2101
        /// </summary>
        II2cPeripheral i2cPeripherl { get; }

        /// <summary>
        /// Create a new EMC2101 object
        /// </summary>
        /// <param name="i2cBus">I2CBus connected to display</param>
        /// <param name="address">Address of the EMC2101 (default = 0x4C)</param>
        public Emc2101(II2cBus i2cBus, byte address = (byte)Address.Default)
        {
            i2cPeripherl = new I2cPeripheral(i2cBus, address);
        }
    }
}