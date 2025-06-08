using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represent an MCP23017 I2C port expander
    /// </summary>
    public class Mcp23017 : Mcp23x1x
    {
        /// <summary>
        /// The default I2C bus address (0x20) for the MCP23017
        /// </summary>
        /// <remarks>
        /// The 7-bit address is set with 3 user-settable address pins: 0b0100[A2][A1][A0] 
        /// </remarks>
        public const byte DefaultAddress = 0b010_0000;

        /// <summary>
        /// Creates an Mcp23017 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        /// <param name="interruptPort">The interrupt port</param>
        /// <param name="resetPort">Optional Meadow output port used to reset the mcp expander</param>
        public Mcp23017(II2cBus i2cBus, byte address = DefaultAddress, IDigitalInterruptPort? interruptPort = null, IDigitalOutputPort? resetPort = null) :
            base(i2cBus, address, interruptPort, resetPort)
        { }

        /// <summary>
        /// Creates an Mcp23017 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="addressBits">The states of the 3 address pins</param>
        /// <param name="interruptPort">The interrupt port</param>
        /// <param name="resetPort">Optional Meadow output port used to reset the mcp expander</param>
        public Mcp23017(II2cBus i2cBus, (bool A0, bool A1, bool A2) addressBits, IDigitalInterruptPort? interruptPort = null, IDigitalOutputPort? resetPort = null) :
            base(i2cBus, CalculateAddress(addressBits), interruptPort, resetPort)
        { }

        private static byte CalculateAddress((bool A0, bool A1, bool A2) addressBits)
        {
            var address = DefaultAddress;
            if (addressBits.A2) { address |= (1 << 2); }
            if (addressBits.A1) { address |= (1 << 1); }
            if (addressBits.A0) { address |= (1 << 0); }
            return address;
        }
    }
}