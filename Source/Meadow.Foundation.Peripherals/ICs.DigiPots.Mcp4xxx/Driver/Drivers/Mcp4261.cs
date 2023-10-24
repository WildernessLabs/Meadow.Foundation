using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.ICs.DigiPots;

/// <summary>
/// Represents an MCP4261 digital potentiometer.
/// </summary>
public class Mcp4261 : Mcp4xx1
{
    /// <inheritdoc/>
    public override int MaxSteps => 257;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mcp4261"/> class.
    /// </summary>
    /// <param name="spiBus">The SPI bus to which the MCP4261 is connected.</param>
    /// <param name="chipSelect">The digital output port for the chip select (CS) pin.</param>
    /// <param name="maxResistance">The maximum resistance of the potentiometer.</param>
    public Mcp4261(ISpiBus spiBus, IDigitalOutputPort chipSelect, Resistance maxResistance) :
        base(spiBus, chipSelect, 2, maxResistance)
    {
    }
}
