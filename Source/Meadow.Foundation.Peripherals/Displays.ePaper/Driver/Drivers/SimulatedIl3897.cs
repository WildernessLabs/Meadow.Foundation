using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated Il3897 display renderer
/// </summary>
public class SimulatedIl3897 : SimulatedDisplayBase
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
    /// Create a new simulated Il3897 display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>
    public SimulatedIl3897(IResizablePixelDisplay displayRenderer,
        bool rotate = true)
        : base(displayRenderer, 122, 250, rotate, ColorMode.Format1bpp)
    { }

    /// <summary>
    /// Create a new simulated Il3897 display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedIl3897(IResizablePixelDisplay displayRenderer)
        : this(displayRenderer, true)
    { }
}