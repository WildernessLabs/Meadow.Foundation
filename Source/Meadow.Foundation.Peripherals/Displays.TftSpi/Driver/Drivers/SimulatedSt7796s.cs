using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated St7796s displayRenderer
/// </summary>
public class SimulatedSt7796s : SimulatedDisplayBase
{
    /// <summary>
    /// The color modes supported by the display
    /// </summary>
    public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565 | ColorMode.Format12bppRgb444;

    /// <summary>
    /// Create a new simulated St7796s displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>rotate
    /// <param name="colorMode"></param>
    public SimulatedSt7796s(IResizablePixelDisplay displayRenderer,
        bool rotate = true,
        ColorMode colorMode = ColorMode.Format12bppRgb444)
        : base(displayRenderer, 320, 480, rotate, colorMode)
    { }

    /// <summary>
    /// Create a new simulated St7796s displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedSt7796s(IResizablePixelDisplay displayRenderer)
        : base(displayRenderer, 320, 480, true, ColorMode.Format12bppRgb444)
    { }
}