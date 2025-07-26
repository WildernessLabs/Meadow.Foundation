using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a control that uses a buffered background for rendering.
/// </summary>
/// <remarks>This abstract class provides a base for controls that require a buffered background to optimize
/// rendering. Derived classes must implement the <see cref="OnDrawBackground(MicroGraphics)"/> method to define how the
/// background is drawn. The control automatically manages the creation and reuse of the background buffer.</remarks>
public abstract class BufferedBackgroundControl : Control
{
    private IPixelBuffer? _backgroundBuffer;
    private ColorMode _colorMode = ColorMode.FormatUnknown;

    /// <summary>
    /// Initializes a new instance of the <see cref="BufferedBackgroundControl"/> class with the specified position and
    /// size.
    /// </summary>
    /// <param name="left">The x-coordinate of the left edge of the control.</param>
    /// <param name="top">The y-coordinate of the top edge of the control.</param>
    /// <param name="width">The width of the control, in pixels.</param>
    /// <param name="height">The height of the control, in pixels.</param>
    protected BufferedBackgroundControl(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
    }

    /// <summary>
    /// Gets the color mode of the display containing this contro
    /// </summary>
    /// <returns></returns>
    protected ColorMode ColorMode => _colorMode;

    /// <summary>
    /// Draws the background of the control or component.
    /// </summary>
    /// <remarks>This method is abstract and must be implemented by derived classes to define how the
    /// background is rendered. The provided <paramref name="graphics"/> object should be used to perform all drawing
    /// operations.</remarks>
    /// <param name="graphics">The <see cref="MicroGraphics"/> instance used to perform the drawing operations.</param>
    protected abstract void OnDrawBackground(MicroGraphics graphics);

    /// <inheritdoc/>
    protected override void OnDraw(MicroGraphics graphics)
    {
        if (_colorMode == ColorMode.FormatUnknown)
        {
            _colorMode = graphics.ColorMode;
        }

        if (_backgroundBuffer == null)
        {
            _backgroundBuffer = MicroGraphics.CreatePixelBuffer(_colorMode, Width, Height);

            var g = new MicroGraphics(_backgroundBuffer, false);
            OnDrawBackground(g);
        }

        if (_backgroundBuffer != null)
        {
            graphics.DrawBuffer(0, 0, _backgroundBuffer);
        }
    }
}
