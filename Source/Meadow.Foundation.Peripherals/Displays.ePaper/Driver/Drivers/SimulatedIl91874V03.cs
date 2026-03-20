using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated Il91874V03 display renderer
/// </summary>
public class SimulatedIl91874V03 : SimulatedDisplayBase
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
    /// Create a new simulated Il91874V03 display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>
    public SimulatedIl91874V03(IResizablePixelDisplay displayRenderer,
        bool rotate = true)
        : base(displayRenderer, 176, 264, rotate, ColorMode.Format1bpp)
    { }

    /// <summary>
    /// Create a new simulated Il91874V03 display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedIl91874V03(IResizablePixelDisplay displayRenderer)
        : this(displayRenderer, true)
    { }
}