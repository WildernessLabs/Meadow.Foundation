using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Sample named virtual display
/// </summary>
public class Ili9341Virtual : VirtualDisplayBase
{
    public Ili9341Virtual(RotationType rotationType = RotationType._270Degrees)
        : base(240, 320, rotationType)
    {
    }
}
