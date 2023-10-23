using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.ICs.DigiPots;

/// <summary>
/// Represents a Mcp4xx1 digital potentiometer
/// </summary>
public abstract class Mcp4xx1 : Mcp4xxx
{ // potentiometer
    protected Mcp4xx1(ISpiBus spiBus, IDigitalOutputPort chipSelect, int resistorCount, Resistance maxResistance, int resolutionSteps)
        : base(spiBus, chipSelect, resistorCount, maxResistance, resolutionSteps)
    {
    }
}
