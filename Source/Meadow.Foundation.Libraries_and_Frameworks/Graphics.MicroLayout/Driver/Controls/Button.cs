namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a clickable display button in the user interface.
/// </summary>
public class Button : ClickableControl
{
    private const int ButtonDepth = 2; // TODO: make this settable?
    private string _text = string.Empty;
    private Image? _image;
    private Color _foregroundColor = Color.Gray;
    private Color _pressedColor;
    private Color _highlightColor;
    private Color _shadowColor;
    private Color _textColor;
    private IFont? _font;
    private ScaleFactor _scaleFactor = ScaleFactor.X1;

    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class with the specified dimensions.
    /// </summary>
    /// <param name="width">The width of the button.</param>
    /// <param name="height">The height of the button.</param>
    /// <param name="text">The initial Text for the control</param>
    public Button(int width, int height, string text = nameof(Button))
        : base(0, 0, width, height)
    {
        Text = text;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class with the specified dimensions.
    /// </summary>
    /// <param name="left">The left coordinate of the button.</param>
    /// <param name="top">The top coordinate of the button.</param>
    /// <param name="width">The width of the button.</param>
    /// <param name="height">The height of the button.</param>
    /// <param name="scaleFactor">The scale factor used for drawing text</param>
    public Button(int left, int top, int width, int height, ScaleFactor scaleFactor = ScaleFactor.X1)
        : base(left, top, width, height)
    {
        ScaleFactor = scaleFactor;
    }

    /// <summary>
    /// Applies the specified display theme to the display button.
    /// </summary>
    /// <param name="theme">The display theme to apply.</param>
    public override void ApplyTheme(DisplayTheme theme)
    {
        if (theme != null)
        {
            if (theme.ForegroundColor != null) this.ForegroundColor = theme.ForegroundColor.Value;
            if (theme.PressedColor != null) this.PressedColor = theme.PressedColor.Value;
            if (theme.HighlightColor != null) this.HighlightColor = theme.HighlightColor.Value;
            if (theme.ShadowColor != null) this.ShadowColor = theme.ShadowColor.Value;
            if (theme.TextColor != null) this.TextColor = theme.TextColor.Value;

            this.Font = theme.Font;
        }
    }

    /// <summary>
    /// Gets or sets the foreground color of the button when not pressed.
    /// </summary>
    public Color ForegroundColor
    {
        get => _foregroundColor;
        set => SetInvalidatingProperty(ref _foregroundColor, value);
    }

    /// <summary>
    /// Gets or sets the foreground color of the button when pressed.
    /// </summary>
    public Color PressedColor
    {
        get => _pressedColor;
        set => SetInvalidatingProperty(ref _pressedColor, value);
    }

    /// <summary>
    /// Gets or sets the highlight color of the button.
    /// </summary>
    public Color HighlightColor
    {
        get => _highlightColor;
        set => SetInvalidatingProperty(ref _highlightColor, value);
    }

    /// <summary>
    /// Gets or sets the shadow color of the button.
    /// </summary>
    public Color ShadowColor
    {
        get => _shadowColor;
        set => SetInvalidatingProperty(ref _shadowColor, value);
    }

    /// <summary>
    /// Gets or sets the text color of the button.
    /// </summary>
    public Color TextColor
    {
        get => _textColor;
        set => SetInvalidatingProperty(ref _textColor, value);
    }

    /// <summary>
    /// Gets or sets the text to be displayed on the button.
    /// </summary>
    public string Text
    {
        get => _text;
        set => SetInvalidatingProperty(ref _text, value);
    }

    /// <summary>
    /// Gets or sets the image to be displayed on the button.
    /// </summary>
    public Image? Image
    {
        get => _image;
        set => SetInvalidatingProperty(ref _image, value);
    }

    /// <summary>
    /// Gets or sets the font used for displaying the text on the button.
    /// </summary>
    public IFont? Font
    {
        get => _font;
        set => SetInvalidatingProperty(ref _font, value);
    }

    /// <summary>
    /// ScaleFactor used to calculate drawn text size
    /// </summary>
    public ScaleFactor ScaleFactor
    {
        get => _scaleFactor;
        set => SetInvalidatingProperty(ref _scaleFactor, value);
    }

    /// <summary>
    /// Draws the display button on the specified <see cref="MicroGraphics"/> surface.
    /// </summary>
    /// <param name="graphics">The <see cref="MicroGraphics"/> surface to draw the button on.</param>
    protected override void OnDraw(MicroGraphics graphics)
    {
        graphics.Stroke = ButtonDepth;

        if (Pressed)
        {
            graphics.DrawRectangle(ScreenLeft, ScreenTop, Width, Height, PressedColor, true);

            graphics.DrawHorizontalLine(ScreenLeft, ScreenTop, Width, ShadowColor);
            graphics.DrawVerticalLine(ScreenLeft, ScreenTop, Height, ShadowColor);

            graphics.DrawHorizontalLine(ScreenLeft, ScreenBottom - 1, Width, HighlightColor);
            graphics.DrawVerticalLine(ScreenRight - 1, ScreenTop, Height, HighlightColor);

            if (Image != null) // image always wins over text
            {
                graphics.DrawImage(
                    ScreenLeft + ((this.Width - Image.Width) / 2) + ButtonDepth,
                    ScreenTop + ((this.Height - Image.Height) / 2) + ButtonDepth,
                    Image);
            }
            else if (!string.IsNullOrEmpty(Text))
            {
                graphics.DrawText(
                    ScreenLeft + ButtonDepth + (this.Width / 2),
                    ScreenTop + ButtonDepth + (this.Height / 2),
                    Text,
                    TextColor,
                    scaleFactor: ScaleFactor,
                    alignmentH: HorizontalAlignment.Center,
                    alignmentV: VerticalAlignment.Center,
                    font: Font);
            }
        }
        else
        {
            graphics.DrawRectangle(ScreenLeft, ScreenTop, Width, Height, ForegroundColor, true);

            graphics.DrawHorizontalLine(ScreenLeft, ScreenTop, Width, HighlightColor);
            graphics.DrawVerticalLine(ScreenLeft, ScreenTop, Height, HighlightColor);

            graphics.DrawHorizontalLine(ScreenLeft, ScreenBottom - 1, Width, ShadowColor);
            graphics.DrawVerticalLine(ScreenRight - 1, ScreenTop, Height, ShadowColor);

            if (Image != null) // image always wins over text
            {
                graphics.DrawImage(
                    ScreenLeft + ((Width - Image.Width) / 2),
                    ScreenTop + ((Height - Image.Height) / 2),
                    Image);
            }
            else if (!string.IsNullOrEmpty(Text))
            {
                graphics.DrawText(
                    ScreenLeft + (Width / 2),
                    ScreenTop + (Height / 2),
                    Text,
                    TextColor,
                    scaleFactor: ScaleFactor,
                    alignmentH: HorizontalAlignment.Center,
                    alignmentV: VerticalAlignment.Center,
                    font: Font);
            }
        }
    }
}
