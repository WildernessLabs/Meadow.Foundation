using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Sample named virtual display
///
/// TODO: Move this to the TFT Displays project after we're happy with implementation. Only in here right now for
/// convenience as a WiP.
/// </summary>
public class Ili9341Virtual : VirtualDisplayBase
{
    public Ili9341Virtual(RotationType rotationType = RotationType._270Degrees)
        : base(240, 320, rotationType)
    {
    }
}
