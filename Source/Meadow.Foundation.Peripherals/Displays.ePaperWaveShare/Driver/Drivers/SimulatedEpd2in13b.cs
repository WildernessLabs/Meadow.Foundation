using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// A virtual display renderer instance of the Epd2in13b epaper display
/// </summary>
public class SimulatedEpd2in13b : SimulatedIl0373
{
    /// <summary>
    /// Create a new instance of the simulated Epd2in13b display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>
    public SimulatedEpd2in13b(IResizablePixelDisplay displayRenderer, bool rotate = true)
        : base(displayRenderer, rotate)
    { }

    /// <summary>
    /// Create a new instance of the simulated Epd2in13b display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedEpd2in13b(IResizablePixelDisplay displayRenderer)
        : this(displayRenderer, true)
    { }
}