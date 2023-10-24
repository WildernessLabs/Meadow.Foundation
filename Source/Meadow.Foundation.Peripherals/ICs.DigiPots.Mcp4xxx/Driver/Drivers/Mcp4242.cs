using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.ICs.DigiPots;

/// <summary>
/// Represents an MCP4242 digital rheostat.
/// </summary>
public class Mcp4242 : Mcp4xx2
{
    /// <inheritdoc/>
    public override int MaxSteps => 129;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mcp4242"/> class.
    /// </summary>
    /// <param name="spiBus">The SPI bus to which the MCP4242 is connected.</param>
    /// <param name="chipSelect">The digital output port for the chip select (CS) pin.</param>
    /// <param name="maxResistance">The maximum resistance of the rheostat.</param>
    public Mcp4242(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 2, maxResistance)
    {
    }
}
