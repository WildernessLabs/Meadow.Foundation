using Meadow.Units;

namespace Meadow.Foundation.ICs.DigiPots;

public interface IPotentiometer
{
    Resistance MaxResistance { get; }
    Resistance Resistance { get; set; }
}
