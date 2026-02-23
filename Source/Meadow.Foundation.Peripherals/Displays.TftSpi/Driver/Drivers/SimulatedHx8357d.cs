using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated Hx8357d displayRenderer
/// </summary>
public class SimulatedHx8357d : SimulatedDisplayBase
{
    /// <summary>
    /// The color modes supported by the display
    /// </summary>
    public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565;

    /// <summary>
    /// Create a new simulated Hx8357d displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>
    /// <param name="colorMode"></param>
    public SimulatedHx8357d(IResizablePixelDisplay displayRenderer,
        bool rotate = true,
        ColorMode colorMode = ColorMode.Format16bppRgb565)
        : base(displayRenderer, 240, 240, rotate, colorMode)
    { }

    /// <summary>
    /// Create a new simulated Hx8357d displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedHx8357d(IResizablePixelDisplay displayRenderer)
        : base(displayRenderer, 240, 240, true, ColorMode.Format16bppRgb565)
    { }
}