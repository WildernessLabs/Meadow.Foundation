using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated Ssd1351 displayRenderer
/// </summary>
public class SimulatedSsd1351 : SimulatedDisplayBase
{
    /// <summary>
    /// The color modes supported by the display
    /// </summary>
    public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565;

    /// <summary>
    /// Create a new simulated Ssd1351 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>rotate
    /// <param name="colorMode"></param>
    public SimulatedSsd1351(IResizablePixelDisplay displayRenderer,
        bool rotate = true,
        ColorMode colorMode = ColorMode.Format16bppRgb565)
        : base(displayRenderer, 128, 128, rotate, colorMode)
    { }

    /// <summary>
    /// Create a new simulated Ssd1351 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedSsd1351(IResizablePixelDisplay displayRenderer)
        : base(displayRenderer, 128, 128, true, ColorMode.Format16bppRgb565)
    { }
}