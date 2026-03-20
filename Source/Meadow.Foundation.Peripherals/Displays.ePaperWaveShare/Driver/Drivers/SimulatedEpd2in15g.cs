using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// A virtual display renderer instance of the Epd2in15g epaper display
/// </summary>
public class SimulatedEpd2in15g : SimulatedDisplayBase
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
    /// Create a new instance of the simulated Epd2in15g display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>
    public SimulatedEpd2in15g(IResizablePixelDisplay displayRenderer,
        bool rotate = true)
        : base(displayRenderer, 160, 296, rotate, ColorMode.Format2bppIndexed)
    {
        if (pixelBufferSimulated is BufferIndexed2 buffer)
        {
            buffer.IndexedColors[0] = Color.Black;
            buffer.IndexedColors[1] = Color.White;
            buffer.IndexedColors[2] = Color.Yellow;
            buffer.IndexedColors[3] = Color.Red;
        }
    }

    /// <summary>
    /// Create a new instance of the simulated Epd2in15g display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedEpd2in15g(IResizablePixelDisplay displayRenderer)
        : this(displayRenderer, true)
    { }
}