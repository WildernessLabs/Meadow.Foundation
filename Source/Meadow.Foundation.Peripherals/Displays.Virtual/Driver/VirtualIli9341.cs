using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

public class VirtualIli9341 : VirtualDisplayBase
{
    public VirtualIli9341(RotationType rotationType = RotationType._270Degrees)
        : base(240, 320, rotationType)
    {
    }
}
