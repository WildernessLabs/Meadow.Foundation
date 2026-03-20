using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated Ili9488 displayRenderer
/// </summary>
public class SimulatedIli9488 : SimulatedDisplayBase
{
    /// <summary>
    /// The color modes supported by the display
    /// </summary>
    public override ColorMode SupportedColorModes => ColorMode.Format24bppRgb888;

    /// <summary>
    /// Create a new simulated Ili9488 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>rotate
    /// <param name="colorMode"></param>
    public SimulatedIli9488(IResizablePixelDisplay displayRenderer,
        bool rotate = true,
        ColorMode colorMode = ColorMode.Format24bppRgb888)
        : base(displayRenderer, 320, 480, rotate, colorMode)
    { }

    /// <summary>
    /// Create a new simulated Ili9488 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedIli9488(IResizablePixelDisplay displayRenderer)
        : base(displayRenderer, 320, 480, true, ColorMode.Format24bppRgb888)
    { }
}