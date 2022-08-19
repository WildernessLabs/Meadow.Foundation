using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represent an MCP23008 SPI port expander
    /// </summary>
    public class Mcp23s08 : Mcp23x08
    {
        /// <summary>
        /// Creates an Mcp23s08 object
        /// </summary>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPort">Chip select port</param>
        /// <param name="interruptPort">optional interupt port, needed for input interrupts</param>
        public Mcp23s08(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, IDigitalInputPort interruptPort = null) : 
            base(spiBus, chipSelectPort, interruptPort)
        {
        }

        /// <summary>
        /// Creates an Mcp23s08 object
        /// </summary>
        /// <param name="device">The device used to create the ports</param>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="interruptPin">optional interupt pin, needed for input interrupts (InterruptMode: EdgeRising, ResistorMode.InternalPullDown)</param>
        public Mcp23s08(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin interruptPin = null) : 
            this(spiBus, device.CreateDigitalOutputPort(chipSelectPin), (interruptPin == null) ? null : device.CreateDigitalInputPort(interruptPin, InterruptMode.EdgeRising, ResistorMode.InternalPullDown))
        {
        }
    }
}