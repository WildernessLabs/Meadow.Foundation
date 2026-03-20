using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// A virtual display renderer instance of the Epd1in54 epaper display
/// </summary>
public class SimulatedEpd1in54 : SimulatedSsd1608
{
    /// <summary>
    /// Create a new instance of the simulated Epd1in54 display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>
    public SimulatedEpd1in54(IResizablePixelDisplay displayRenderer, bool rotate = true)
        : base(displayRenderer, rotate)
    { }

    /// <summary>
    /// Create a new instance of the simulated Epd1in54 display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedEpd1in54(IResizablePixelDisplay displayRenderer)
        : this(displayRenderer, true)
    { }
}