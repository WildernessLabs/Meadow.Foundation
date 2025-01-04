using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// A virtual display instance of the Epd5in65 epaper display.
///
/// TODO: Move to epapers project when finished.
/// </summary>
public class Epd5in65fVirtual : VirtualDisplayBase
{
    public Epd5in65fVirtual() : base(600, 448, RotationType.Normal, ColorMode.Format4bppIndexed)
    {
        base.SupportedColorModes = ColorMode.Format4bppIndexed;
    }
}