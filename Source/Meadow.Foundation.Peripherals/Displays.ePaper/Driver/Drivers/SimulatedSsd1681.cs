using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated Ssd1681 display renderer
/// </summary>
public class SimulatedSsd1681 : SimulatedDisplayBase
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
    /// Create a new simulated Ssd1681 display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>
    public SimulatedSsd1681(IResizablePixelDisplay displayRenderer,
        bool rotate = true)
        : base(displayRenderer, 200, 200, rotate, ColorMode.Format2bppIndexed)
    {
        if (pixelBufferSimulated is BufferIndexed2 buffer)
        {
            buffer.IndexedColors[0] = Color.Black;
            buffer.IndexedColors[1] = Color.White;
            buffer.IndexedColors[2] = Color.Red;
        }
    }

    /// <summary>
    /// Create a new simulated Ssd1681 display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedSsd1681(IResizablePixelDisplay displayRenderer)
        : this(displayRenderer, true)
    { }
}