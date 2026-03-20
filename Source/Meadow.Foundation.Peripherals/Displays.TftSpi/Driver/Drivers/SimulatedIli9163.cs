using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated Ili9163 displayRenderer
/// </summary>
public class SimulatedIli9163 : SimulatedDisplayBase
{
    /// <summary>
    /// The color modes supported by the display
    /// </summary>
    public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565 | ColorMode.Format12bppRgb444;

    /// <summary>
    /// Create a new simulated Ili9163 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>
    /// <param name="colorMode"></param>
    public SimulatedIli9163(IResizablePixelDisplay displayRenderer,
        bool rotate = true,
        ColorMode colorMode = ColorMode.Format12bppRgb444)
        : base(displayRenderer, 240, 320, rotate, colorMode)
    { }

    /// <summary>
    /// Create a new simulated Ili9163 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedIli9163(IResizablePixelDisplay displayRenderer)
        : base(displayRenderer, 240, 320, true, ColorMode.Format12bppRgb444)
    { }
}