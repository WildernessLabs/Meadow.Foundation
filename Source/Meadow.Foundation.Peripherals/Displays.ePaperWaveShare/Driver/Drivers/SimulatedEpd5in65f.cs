using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// A virtual display renderer instance of the Epd5in65f epaper display
/// </summary>
public class SimulatedEpd5in65f : SimulatedDisplayBase
{
    /// <inheritdoc/>
    public override ColorMode SupportedColorModes => ColorMode.Format4bppIndexed;

    /// <inheritdoc/>
    protected override Color EnabledColor => Color.Black;

    /// <inheritdoc/>
    protected override Color DisabledColor => Color.White;

    /// <summary>
    /// Create a new instance of the simulated Epd5in65f display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>rotate
    public SimulatedEpd5in65f(IResizablePixelDisplay displayRenderer, bool rotate = true)
        : base(displayRenderer, 600, 448, rotate, ColorMode.Format4bppIndexed)
    {
        SupportedColorModes = ColorMode.Format4bppIndexed;

        if (pixelBufferSimulated is BufferIndexed4 buffer)
        {
            buffer.IndexedColors[0] = Color.Black;
            buffer.IndexedColors[1] = Color.White;
            buffer.IndexedColors[2] = Color.Green;
            buffer.IndexedColors[3] = Color.Blue;
            buffer.IndexedColors[4] = Color.Red;
            buffer.IndexedColors[5] = Color.Yellow;
            buffer.IndexedColors[6] = Color.Orange;
        }
    }

    /// <summary>
    /// Create a new instance of the simulated Epd5in65f display renderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedEpd5in65f(IResizablePixelDisplay displayRenderer)
        : this(displayRenderer, true)
    { }
}