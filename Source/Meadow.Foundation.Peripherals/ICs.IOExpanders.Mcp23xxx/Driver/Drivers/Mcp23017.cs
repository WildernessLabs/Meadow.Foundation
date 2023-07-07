using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represent an MCP23017 I2C port expander
    /// </summary>
    public class Mcp23017 : Mcp23x1x
    {
        /// <summary>
        /// Creates an Mcp23017 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        /// <param name="interruptPort">The interrupt port</param>
        /// <param name="resetPort">Optional Meadow output port used to reset the mcp expander</param>
        public Mcp23017(II2cBus i2cBus, byte address = 32, IDigitalInterruptPort? interruptPort = null, IDigitalOutputPort? resetPort = null) :
            base(i2cBus, address, interruptPort, resetPort)
        { }
    }
}