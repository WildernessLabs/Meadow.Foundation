using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated St7789 displayRenderer
/// </summary>
public class SimulatedSt7789 : SimulatedDisplayBase
{
    /// <summary>
    /// The color modes supported by the display
    /// </summary>
    public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565 | ColorMode.Format12bppRgb444 | ColorMode.Format18bppRgb666;

    /// <summary>
    /// Create a new simulated St7789 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>rotate
    /// <param name="colorMode"></param>
    public SimulatedSt7789(IResizablePixelDisplay displayRenderer,
        bool rotate = true,
        ColorMode colorMode = ColorMode.Format12bppRgb444)
        : base(displayRenderer, 240, 240, rotate, colorMode)
    { }

    /// <summary>
    /// Create a new simulated St7789 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedSt7789(IResizablePixelDisplay displayRenderer)
        : base(displayRenderer, 240, 240, true, ColorMode.Format12bppRgb444)
    { }
}