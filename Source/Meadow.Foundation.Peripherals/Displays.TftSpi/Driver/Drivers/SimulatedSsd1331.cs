using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated Ssd1331 displayRenderer
/// </summary>
public class SimulatedSsd1331 : SimulatedDisplayBase
{
    /// <summary>
    /// The color modes supported by the display
    /// </summary>
    public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565;

    /// <summary>
    /// Create a new simulated Ssd1331 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>rotate
    /// <param name="colorMode"></param>
    public SimulatedSsd1331(IResizablePixelDisplay displayRenderer,
        bool rotate = true,
        ColorMode colorMode = ColorMode.Format16bppRgb565)
        : base(displayRenderer, 96, 64, rotate, colorMode)
    { }

    /// <summary>
    /// Create a new simulated Ssd1331 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedSsd1331(IResizablePixelDisplay displayRenderer)
        : base(displayRenderer, 96, 64, true, ColorMode.Format16bppRgb565)
    { }
}