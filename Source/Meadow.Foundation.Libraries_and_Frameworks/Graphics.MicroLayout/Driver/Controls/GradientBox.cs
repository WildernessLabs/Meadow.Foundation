namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a rectangular gradient display box in the user interface.
/// </summary>
public class GradientBox : ThemedControl
{
    /// <summary>
    /// Gets or sets a value indicating whether the gradient direction is horizontal or vertical.
    /// </summary>
    public bool IsHorizontal { get; set; } = true;

    /// <summary>
    /// Gets or sets the start color of the gradient box. Left color if Horizontal, Top color if vertical.
    /// </summary>
    public Color StartColor
    {
        get => _startColor;
        set => SetInvalidatingProperty(ref _startColor, value);
    }
    private Color _startColor;

    /// <summary>
    /// Gets or sets the start color of the gradient box. Right color if Horizontal, Bottom color if vertical.
    /// </summary>
    public Color EndColor
    {
        get => _endColor;
        set => SetInvalidatingProperty(ref _endColor, value);
    }
    private Color _endColor;

    /// <summary>
    /// Initializes a new instance of the <see cref="Box"/> class with the specified dimensions.
    /// </summary>
    /// <param name="left">The left coordinate of the display box.</param>
    /// <param name="top">The top coordinate of the display box.</param>
    /// <param name="width">The width of the display box.</param>
    /// <param name="height">The height of the display box.</param>
    public GradientBox(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
    }

    /// <summary>
    /// Applies the specified display theme to the display gradient box.
    /// </summary>
    /// <param name="theme">The display theme to apply.</param>
    public override void ApplyTheme(DisplayTheme theme)
    {
        if (theme != null)
        {
            if (theme.StartColor != null) this.StartColor = theme.StartColor.Value;
            if (theme.EndColor != null) this.EndColor = theme.EndColor.Value;
        }
    }

    /// <summary>
    /// Draws the display gradient box on the specified <see cref="MicroGraphics"/> surface.
    /// </summary>
    /// <param name="graphics">The <see cref="MicroGraphics"/> surface to draw the display box on.</param>
    protected override void OnDraw(MicroGraphics graphics)
    {
        if (StartColor != Color.Transparent || EndColor != Color.Transparent)
        {
            if (IsHorizontal)
            {
                graphics.DrawHorizontalGradient(Left + (Parent?.Left ?? 0), Top + (Parent?.Top ?? 0), Width, Height, StartColor, EndColor);
            }
            else
            {
                graphics.DrawVerticalGradient(Left + (Parent?.Left ?? 0), Top + (Parent?.Top ?? 0), Width, Height, StartColor, EndColor);
            }
        }
    }
}