using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated S6D02A1 displayRenderer
/// </summary>
public class SimulatedS6D02A1 : SimulatedDisplayBase
{
    /// <summary>
    /// The color modes supported by the display
    /// </summary>
    public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565 | ColorMode.Format12bppRgb444;

    /// <summary>
    /// Create a new simulated S6D02A1 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>rotate
    /// <param name="colorMode"></param>
    public SimulatedS6D02A1(IResizablePixelDisplay displayRenderer,
        bool rotate = true,
        ColorMode colorMode = ColorMode.Format12bppRgb444)
        : base(displayRenderer, 128, 160, rotate, colorMode)
    { }

    /// <summary>
    /// Create a new simulated S6D02A1 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedS6D02A1(IResizablePixelDisplay displayRenderer)
        : base(displayRenderer, 128, 160, true, ColorMode.Format12bppRgb444)
    { }
}