using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// MCP3001 Analog to Digital Converter (ADC)
    /// </summary>
    public partial class Mcp3001 : Mcp3xxx
    {
        /// <summary>
        /// The pins
        /// </summary>
        public PinDefinitions Pins { get; }

        /// <summary>
        /// Constructs Mcp3001 instance
        /// </summary>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        public Mcp3001(ISpiBus spiBus, IPin chipSelectPin)
            : base(spiBus, chipSelectPin, 1, 10)
        {
            Pins = new PinDefinitions(this);
        }

        /// <summary>
        /// Constructs Mcp3001 instance
        /// </summary>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPort">Chip select port</param>
        public Mcp3001(ISpiBus spiBus, IDigitalOutputPort chipSelectPort)
            : base(spiBus, chipSelectPort, 1, 10)
        {
            Pins = new PinDefinitions(this);
        }

        /// <inheritdoc />
        public override bool IsInputTypeSupported(InputType inputType)
        {
            if (inputType == InputType.SingleEnded)
                return false;

            return base.IsInputTypeSupported(inputType);
        }

        /// <summary>
        /// Create an analog input port for a pin
        /// </summary>
        public IAnalogInputPort CreateAnalogInputPort(int sampleCount = 64)
        {
            return CreateAnalogInputPort(sampleCount, TimeSpan.FromSeconds(1), new Voltage(0));
        }

        /// <summary>
        /// Create an analog input port for a pin
        /// </summary>
        public IAnalogInputPort CreateAnalogInputPort(int sampleCount, TimeSpan sampleInterval, Voltage voltageReference)
        {
            var channel = Pins.INPlus.SupportedChannels.OfType<IAnalogChannelInfo>().FirstOrDefault();

            return new AnalogInputPort(this, Pins.INPlus, channel, sampleCount);
        }
    }
}