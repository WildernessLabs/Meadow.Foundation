using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// MCP3002 Analog to Digital Converter (ADC)
    /// </summary>
    public partial class Mcp3002 : Mcp3xxx
    {
        /// <summary>
        /// The pins
        /// </summary>
        public PinDefinitions Pins { get; }

        /// <summary>
        /// Constructs Mcp3002 instance
        /// </summary>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPort">Chip select port</param>
        public Mcp3002(ISpiBus spiBus, IDigitalOutputPort chipSelectPort)
            : base(spiBus, chipSelectPort, 2, 10)
        {
            Pins = new PinDefinitions(this);
        }
    }
}
