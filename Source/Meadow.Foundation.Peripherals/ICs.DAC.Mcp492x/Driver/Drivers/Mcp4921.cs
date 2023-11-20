using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.DAC;

/// <summary>
/// Represents the MCP4921 digital-to-analog converter (DAC) implementation,
/// inheriting from the base Mcp492x class.
/// </summary>
public class Mcp4921 : Mcp492x
{
    /// <summary>
    /// Gets the pin definitions for the MCP4921.
    /// </summary>
    public PinDefinitions Pins { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mcp4921"/> class.
    /// </summary>
    /// <param name="spiBus">The SPI bus.</param>
    /// <param name="chipSelectPort">The chip select port.</param>
    public Mcp4921(ISpiBus spiBus, IDigitalOutputPort chipSelectPort)
        : base(spiBus, chipSelectPort)
    {
        Pins = new PinDefinitions(this);
    }

    /// <summary>
    /// Represents the pin definitions for the MCP4921 DAC.
    /// </summary>
    public class PinDefinitions : IPinDefinitions
    {
        /// <summary>
        /// Analog-digital converter precision
        /// </summary>
        public virtual byte DACPrecisionBits => 12;

        /// <summary>
        /// Collection of pins
        /// </summary>
        public IList<IPin> AllPins { get; } = new List<IPin>();

        /// <inheritdoc/>
        public IPinController? Controller { get; set; }

        /// <summary>
        /// Create a new PinDefinitions object
        /// </summary>
        public PinDefinitions(Mcp492x mcp)
        {
            Controller = mcp;
        }

        /// <summary>
        /// Channel A pin
        /// </summary>
        public IPin ChannelA => new Pin(
            Controller,
            "A",
            Channel.ChannelA,
            new List<IChannelInfo> {
                new AnalogChannelInfo("A", DACPrecisionBits, false, true),
            }
        );

        /// <summary>
        /// Get Enumerator
        /// </summary>
        public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}