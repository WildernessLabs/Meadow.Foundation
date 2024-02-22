using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// MCP3002 Analog to Digital Converter (ADC)
    /// </summary>
    public partial class Mcp3201 : Mcp3xxx
    {
        /// <summary>
        /// The pins
        /// </summary>
        public PinDefinitions Pins { get; }

        /// <summary>
        /// Constructs Mcp3201 instance
        /// </summary>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        public Mcp3201(ISpiBus spiBus, IPin chipSelectPin)
            : base(spiBus, chipSelectPin, 1, 12)
        {
            Pins = new PinDefinitions(this);
        }

        /// <summary>
        /// Constructs Mcp3201 instance
        /// </summary>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPort">Chip select port</param>
        public Mcp3201(ISpiBus spiBus, IDigitalOutputPort chipSelectPort)
            : base(spiBus, chipSelectPort, 1, 12)
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
            return CreateAnalogInputPort(sampleCount, TimeSpan.FromSeconds(1), DefaultReferenceVoltage);
        }

        /// <summary>
        /// Create an analog input port for a pin
        /// </summary>
        public IAnalogInputPort CreateAnalogInputPort(int sampleCount, TimeSpan sampleInterval, Voltage voltageReference)
        {
            var channel = Pins.INPlus.SupportedChannels.OfType<IAnalogChannelInfo>().FirstOrDefault();

            return new AnalogInputPort(this, Pins.INPlus, channel, sampleCount, voltageReference);
        }

        /// <summary>
        /// Reads a value from the device
        /// </summary>
        /// <param name="channel">Channel to read - for differential inputs this represents a channel pair (valid values: 0 - channelcount - 1 or 0 - channelcount / 2 - 1  with differential inputs)</param>
        /// <param name="inputType">The type of input channel to read</param>
        /// <param name="adcResolutionBits">The number of bits in the returned value</param>
        /// <returns>A value corresponding to relative voltage level on specified device channel</returns>
        protected override int ReadInternal(int channel, InputType inputType, int adcResolutionBits)
        {
            Span<byte> buffer = stackalloc byte[2];
            SpiComms.Read(buffer);

            // peripheral is 12-bits, HOWEVER, bit 0 is unused, so mask and shift 13 bits first
            var data = (buffer[0] & 0x1f) << 8 | buffer[1];
            // then shift back right 1 bit
            var result = data >> 1;
            return result;
        }
    }
}