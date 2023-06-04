using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// MCP3004 Analog to Digital Converter (ADC)
    /// </summary>
    public partial class Mcp3004 : Mcp3xxx
    {
        /// <summary>
        /// Constructs Mcp3004 instance
        /// </summary>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPort">Chip select port</param>
        public Mcp3004(ISpiBus spiBus, IDigitalOutputPort chipSelectPort)
            : base(spiBus, chipSelectPort, 4, 10)
        {
        }
    }
}