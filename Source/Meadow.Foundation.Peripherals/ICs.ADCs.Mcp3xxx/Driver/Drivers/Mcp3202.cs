using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// MCP3202 Analog to Digital Converter (ADC)
    /// </summary>
    public class Mcp3202 : Mcp3xxx
    {
        /// <summary>
        /// Constructs Mcp3008 instance
        /// </summary>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPort">Chip select port</param>
        public Mcp3202(ISpiBus spiBus, IDigitalOutputPort chipSelectPort)
            : base(spiBus, chipSelectPort, 2, 12)
        {
        }
    }
}
