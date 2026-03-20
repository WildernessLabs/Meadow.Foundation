using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated Ssd1608 display renderer
/// </summary>
public class SimulatedSsd1608 : SimulatedDisplayBase
{
    /// <summary>
    /// The color modes supported by the display
    /// </summary>
    public override ColorMode SupportedColorModes => ColorMode.Format1bpp;

    /// <inheritdoc/>
    protected override Color EnabledColor => Color.Black;

    /// <inheritdoc/>
    protected override Color DisabledColor => Color.White;

    /// <summary>
    /// Create a new simulated Ssd1608 display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>
    public SimulatedSsd1608(IResizablePixelDisplay displayRenderer,
        bool rotate = true)
        : base(displayRenderer, 200, 200, rotate, ColorMode.Format1bpp)
    { }

    /// <summary>
    /// Create a new simulated Ssd1608 display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedSsd1608(IResizablePixelDisplay displayRenderer)
        : this(displayRenderer, true)
    { }
}