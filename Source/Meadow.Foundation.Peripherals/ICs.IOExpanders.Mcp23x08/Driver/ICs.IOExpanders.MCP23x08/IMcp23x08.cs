using Meadow.Foundation.ICs.IOExpanders.Ports;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public interface IMcp23x08 : IMcp23x
    {
        /// <summary>
        /// The list of pins available on the Mcp23x08
        /// </summary>
        McpGpioPort Pins { get; }

        /// <summary>
        /// Reads a byte value from all of the pins. little-endian; the least
        /// significant bit is the value of GP0. So a byte value of 0x60, or
        /// 0110 0000, means that pins GP5 and GP6 are high.
        ///
        /// If any pin on the port is currently configured
        /// as an output, this will change the configuration.
        /// </summary>
        /// <returns>A little-endian byte mask of the pin values.</returns>
        byte ReadPort();

        /// <summary>
        /// Outputs a byte value across all of the pins by writing directly 
        /// to the output latch (OLAT) register.
        ///
        /// If any pin on the port is currently configured
        /// as an input, this will change the configuration.
        /// </summary>
        /// <param name="mask"></param>
        void WritePort(byte mask);
    }
}
