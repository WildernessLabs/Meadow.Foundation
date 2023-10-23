using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.ICs.DigiPots;

/// <summary>
/// Represents a Mcp4xx2 digital rheostat
/// </summary>
public abstract class Mcp4xx2 : Mcp4xxx
{ // rheostat
    protected Mcp4xx2(ISpiBus spiBus, IDigitalOutputPort chipSelect, int resistorCount, Resistance maxResistance, int resolutionSteps)
        : base(spiBus, chipSelect, resistorCount, maxResistance, resolutionSteps)
    {
    }
}
