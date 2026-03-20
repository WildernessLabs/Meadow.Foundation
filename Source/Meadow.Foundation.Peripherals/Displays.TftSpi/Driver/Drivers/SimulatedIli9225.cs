using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated Ili9225 displayRenderer
/// </summary>
public class SimulatedIli9225 : SimulatedDisplayBase
{
    /// <summary>
    /// The color modes supported by the display
    /// </summary>
    public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565 | ColorMode.Format12bppRgb444 | ColorMode.Format18bppRgb666;

    /// <summary>
    /// Create a new simulated Ili9225 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>
    /// <param name="colorMode"></param>
    public SimulatedIli9225(IResizablePixelDisplay displayRenderer,
        bool rotate = true,
        ColorMode colorMode = ColorMode.Format12bppRgb444)
        : base(displayRenderer, 176, 220, rotate, colorMode)
    { }

    /// <summary>
    /// Create a new simulated Ili9225 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedIli9225(IResizablePixelDisplay displayRenderer)
        : base(displayRenderer, 176, 220, true, ColorMode.Format12bppRgb444)
    { }
}