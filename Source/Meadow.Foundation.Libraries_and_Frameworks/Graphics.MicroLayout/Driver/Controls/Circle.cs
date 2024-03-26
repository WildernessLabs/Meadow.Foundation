namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a circle in the user interface.
/// </summary>
public class Circle : ThemedControl
{
    private Color foreColor;
    private Point center;
    private int radius;

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
        : this(new Point(centerX, centerY), radius)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Circle"/> class with the specified dimensions.
    /// </summary>
    /// <param name="center">The coordinate of the circles's center.</param>
    /// <param name="radius">The radius of the circle.</param>
    public Circle(Point center, int radius)
        : base(center.X - radius, center.Y - radius, radius * 2, radius * 2)
    {
        this.center = center;
        this.radius = radius;
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
        get => foreColor;
        set => SetInvalidatingProperty(ref foreColor, value);
    }

    /// <summary>
    /// Gets or sets the foreground color of the Circle.
    /// </summary>
    public Point Center
    {
        get => center;
        set => SetInvalidatingProperty(ref center, value);
    }

    /// <inheritdoc/>
    public override int Left
    {
        get => center.X - radius;
        set
        {
            center.X = value + radius;
            Invalidate();
        }
    }

    /// <inheritdoc/>
    public override int Top
    {
        get => center.Y - radius;
        set
        {
            center.Y = value + radius;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the foreground color of the Circle.
    /// </summary>
    public int Radius
    {
        get => radius;
        set
        {
            radius = value;
            Left = center.X - radius;
            Width = radius * 2;
            Top = center.Y - radius;
            Height = radius * 2;
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
            graphics.DrawCircle(center.X, center.Y, radius, ForeColor, IsFilled);
        }
    }
}
