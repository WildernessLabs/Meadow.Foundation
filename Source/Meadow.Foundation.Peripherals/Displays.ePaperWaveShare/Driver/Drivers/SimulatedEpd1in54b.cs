using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// A virtual display renderer instance of the Epd1in54b epaper display
/// </summary>
public class SimulatedEpd1in54b : SimulatedIl0376F
{
    /// <summary>
    /// Create a new instance of the simulated Epd1in54b display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>
    public SimulatedEpd1in54b(IResizablePixelDisplay displayRenderer, bool rotate = true)
        : base(displayRenderer, rotate)
    { }

    /// <summary>
    /// Create a new instance of the simulated Epd1in54b display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedEpd1in54b(IResizablePixelDisplay displayRenderer)
        : this(displayRenderer, true)
    { }
}