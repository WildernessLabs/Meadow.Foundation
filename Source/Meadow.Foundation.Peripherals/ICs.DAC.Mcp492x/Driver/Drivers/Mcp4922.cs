using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.DAC;

/// <summary>
/// Represents the MCP4921 digital-to-analog converter (DAC) implementation,
/// inheriting from the base Mcp492x class.
/// </summary>
public class Mcp4922 : Mcp492x
{
    /// <summary>
    /// The pins
    /// </summary>
    public PinDefinitions Pins { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mcp4922"/> class.
    /// </summary>
    /// <param name="spiBus">The SPI bus.</param>
    /// <param name="chipSelect">The chip select port.</param>
    public Mcp4922(ISpiBus spiBus, IDigitalOutputPort chipSelect) : base(spiBus, chipSelect)
    {
        Pins = new PinDefinitions(this);
    }

    /// <summary>
    /// Represents the pin definitions for the MCP4922 DAC.
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

        /// <summary>
        /// The pin controller
        /// </summary>
        public IPinController Controller { get; set; }

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
        /// Channel B pin
        /// </summary>
        public IPin ChannelB => new Pin(
            Controller,
            "B",
            Channel.ChannelB,
            new List<IChannelInfo> {
                new AnalogChannelInfo("B", DACPrecisionBits, false, true),
            }
        );

        /// <summary>
        /// Get Enumerator
        /// </summary>
        public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}