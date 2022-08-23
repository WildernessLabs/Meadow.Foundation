using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represent an MCP23018 I2C port expander with open-drain outputs
    /// </summary>
    public class Mcp23018 : Mcp23x1x
    {
        /// <summary>
        /// Creates an Mcp23018 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        /// <param name="interruptPort">The interrupt port</param>
        public Mcp23018(II2cBus i2cBus, byte address = 32, IDigitalInputPort interruptPort = null) :
            base(i2cBus, address, interruptPort)
        { }
    }
}