using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated Ssd1680 display renderer
/// </summary>
public class SimulatedSsd1680 : SimulatedDisplayBase
{
    /// <summary>
    /// The color modes supported by the display
    /// </summary>
    public override ColorMode SupportedColorModes => ColorMode.Format2bppIndexed;

    /// <inheritdoc/>
    protected override Color EnabledColor => Color.Black;

    /// <inheritdoc/>
    protected override Color DisabledColor => Color.White;

    /// <summary>
    /// Create a new simulated Ssd1680 display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>
    public SimulatedSsd1680(IResizablePixelDisplay displayRenderer,
        bool rotate = true)
        : base(displayRenderer, 122, 250, rotate, ColorMode.Format2bppIndexed)
    {
        if (pixelBufferSimulated is BufferIndexed2 buffer)
        {
            buffer.IndexedColors[0] = Color.Black;
            buffer.IndexedColors[1] = Color.White;
            buffer.IndexedColors[2] = Color.Red;
        }
    }

    /// <summary>
    /// Create a new simulated Ssd1680 display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedSsd1680(IResizablePixelDisplay displayRenderer)
        : this(displayRenderer, true)
    { }
}