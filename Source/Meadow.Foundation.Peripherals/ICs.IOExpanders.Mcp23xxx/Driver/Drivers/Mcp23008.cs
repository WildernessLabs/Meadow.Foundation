using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represent an MCP23008 I2C port expander
    /// </summary>
    public class Mcp23008 : Mcp23x08
    {
        /// <summary>
        /// Creates an Mcp23008 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        /// <param name="interruptPort">The interrupt port</param>
        public Mcp23008(II2cBus i2cBus, byte address = 32, IDigitalInputPort interruptPort = null) : 
            base(i2cBus, address, interruptPort)
        { }
    }
}