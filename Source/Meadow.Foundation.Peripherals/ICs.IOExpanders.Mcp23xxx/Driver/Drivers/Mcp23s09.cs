using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represent an MCP23S09 SPI port expander with open-drain outputs
    /// </summary>
    public class Mcp23s09 : Mcp23x0x
    {
        /// <summary>
        /// Creates an Mcp23s09 object
        /// </summary>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPort">Chip select port</param>
        /// <param name="interruptPort">optional interupt port, needed for input interrupts</param>
        /// <param name="resetPort">Optional Meadow output port used to reset the mcp expander</param>
        public Mcp23s09(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, IDigitalInputPort interruptPort = null, IDigitalOutputPort resetPort = null) :
            base(spiBus, chipSelectPort, interruptPort, resetPort)
        {
        }

        /// <summary>
        /// Creates an Mcp23s09 object
        /// </summary>
        /// <param name="device">The device used to create the ports</param>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="interruptPin">optional interupt pin, needed for input interrupts (InterruptMode: EdgeRising, ResistorMode.InternalPullDown)</param>
        public Mcp23s09(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin interruptPin = null) :
            this(spiBus, device.CreateDigitalOutputPort(chipSelectPin), (interruptPin == null) ? null : device.CreateDigitalInputPort(interruptPin, InterruptMode.EdgeRising, ResistorMode.InternalPullDown))
        {
        }

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
                throw new System.ArgumentException("Output type must be OpenDrain for Mcp23S09");
            }

            return base.CreateDigitalOutputPort(pin, initialState, outputType);
        }
    }
}