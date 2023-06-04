using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// MCP3002 Analog to Digital Converter (ADC)
    /// </summary>
    public partial class Mcp3201 : Mcp3xxx
    {
        /// <summary>
        /// Constructs Mcp3201 instance
        /// </summary>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPort">Chip select port</param>
        public Mcp3201(ISpiBus spiBus, IDigitalOutputPort chipSelectPort)
            : base(spiBus, chipSelectPort, 1, 12)
        {
        }

        /// <inheritdoc />
        public override bool IsInputTypeSupported(InputType inputType)
        {
            if (inputType == InputType.SingleEnded)
                return false;

            return base.IsInputTypeSupported(inputType);
        }
    }
}