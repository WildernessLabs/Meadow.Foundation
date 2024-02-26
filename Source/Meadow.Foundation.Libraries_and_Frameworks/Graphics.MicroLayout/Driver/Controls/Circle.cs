namespace Meadow.Foundation.Graphics.MicroLayout;

public struct Coordinate2D
{
    public int X { get; set; }
    public int Y { get; set; }
}

/// <summary>
/// Represents a circle in the user interface.
/// </summary>
public class Circle : ThemedControl
{
    private Color _foreColor;

    /// <summary>
    /// Gets or sets a value indicating whether the Circle is filled with the foreground color.
    /// </summary>
    public bool IsFilled { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="Circle"/> class with the specified dimensions.
    /// </summary>
    /// <param name="centerX">The X coordinate of the circles's center.</param>
    /// <param name="centerY">The Y coordinate of the circles's center.</param>
    /// <param name="radius">The radius of the circle.</param>
    public Circle(int centerX, int centerY, int radius)
        : base(centerX - radius, centerY - radius, radius * 2, radius * 2)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Circle"/> class with the specified dimensions.
    /// </summary>
    /// <param name="center">The coordinate of the circles's center.</param>
    /// <param name="radius">The radius of the circle.</param>
    public Circle(Coordinate2D center, int radius)
        : base(center.X - radius, center.Y - radius, radius * 2, radius * 2)
    {
    }

    /// <summary>
    /// Applies the specified display theme to the Circle.
    /// </summary>
    /// <param name="theme">The display theme to apply.</param>
    public override void ApplyTheme(DisplayTheme theme)
    {
        if (theme != null)
        {
            if (theme.ForegroundColor != null) this.ForeColor = theme.ForegroundColor.Value;
        }
    }

    /// <summary>
    /// Gets or sets the foreground color of the Circle.
    /// </summary>
    public Color ForeColor
    {
        get => _foreColor;
        set => SetInvalidatingProperty(ref _foreColor, value);
    }

    /// <summary>
    /// Gets or sets the foreground color of the Circle.
    /// </summary>
    public int Radius
    {
        get => Width / 2;
        set
        {
            // keep centered
            var coeff = (value > Radius) ? -1 : 1;
            var offset = value - Radius;

            Width = value * 2;
        }
    }

    /// <summary>
    /// Draws the Circle on the specified <see cref="MicroGraphics"/> surface.
    /// </summary>
    /// <param name="graphics">The <see cref="MicroGraphics"/> surface to draw the Circle on.</param>
    protected override void OnDraw(MicroGraphics graphics)
    {
        if (ForeColor != Color.Transparent)
        {
            var radius = (Right - Left) / 2;
            var centerX = Left + radius;
            var centerY = Top + radius;
            graphics.DrawCircle(centerX, centerY, radius, ForeColor, IsFilled);
        }
    }
}
