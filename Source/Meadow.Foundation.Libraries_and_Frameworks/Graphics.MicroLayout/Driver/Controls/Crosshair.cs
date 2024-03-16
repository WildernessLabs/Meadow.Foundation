namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a circle in the user interface.
/// </summary>
public class Crosshair : ThemedControl
{
    private Color _foreColor = Color.Black;
    private int _lineWidth;

    /// <summary>
    /// Initializes a new instance of the <see cref="Crosshair"/> class with the specified dimensions and center point.
    /// </summary>
    /// <param name="centerX">The X coordinate of the crosshair's center.</param>
    /// <param name="centerY">The Y coordinate of the crosshair's center.</param>
    /// <param name="size">The with and height of the crosshair.</param>
    /// <param name="linewidth">The line width of the crosshair</param>
    public Crosshair(int centerX, int centerY, int size = 20, int linewidth = 3)
        : base(centerX, centerY, size, size)
    {
        _lineWidth = linewidth;
    }

    /// <inheritdoc/>
    public override void ApplyTheme(DisplayTheme theme)
    {
        if (theme != null)
        {
            if (theme.ForegroundColor != null) this.ForeColor = theme.ForegroundColor.Value;
        }
    }

    /// <summary>
    /// Gets or sets the foreground color of the Crosshair.
    /// </summary>
    public Color ForeColor
    {
        get => _foreColor;
        set => SetInvalidatingProperty(ref _foreColor, value);
    }

    /// <inheritdoc/>
    protected override void OnDraw(MicroGraphics graphics)
    {
        if (ForeColor != Color.Transparent)
        {
            // position is the center of the crosshair
            graphics.DrawRectangle(Left - Width / 2, Top - _lineWidth / 2, Width, _lineWidth, ForeColor, true);
            graphics.DrawRectangle(Left - _lineWidth / 2, Top - Height / 2, _lineWidth, Height, ForeColor, true);
        }
    }
}
