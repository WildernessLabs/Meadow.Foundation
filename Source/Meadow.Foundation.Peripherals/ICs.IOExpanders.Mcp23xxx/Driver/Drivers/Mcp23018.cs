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

        /// <summary>
        /// Creates a new DigitalOutputPort using the specified pin and initial state
        /// </summary>
        /// <param name="pin">The pin number to create the port on</param>
        /// <param name="initialState">Whether the pin is initially high or low</param>
        /// <returns>IDigitalOutputPort</returns>
        public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false)
        {
            return base.CreateDigitalOutputPort(pin, initialState, OutputType.OpenDrain);
        }

        /// <summary>
        /// Creates a new DigitalOutputPort using the specified pin and initial state
        /// </summary>
        /// <param name="pin">The pin number to create the port on</param>
        /// <param name="initialState">Whether the pin is initially high or low</param>
        /// <param name="outputType">The output type</param>
        /// <returns>IDigitalOutputPort</returns>
        public override IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType outputType = OutputType.OpenDrain)
        {
            if (outputType != OutputType.OpenDrain)
            {
                throw new System.ArgumentException("Output type must be OpenDrain for Mcp23018");
            }

            return base.CreateDigitalOutputPort(pin, initialState, outputType);
        }
    }
}