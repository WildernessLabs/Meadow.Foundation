namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a two row label display control in the user interface.
/// </summary>
public class DoubleLabel : ClickableControl
{
    /// <summary>
    /// Gets or sets the horizontal alignment of the label text within the label display control.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment
    {
        get => _horizontalAlignment;
        set => SetInvalidatingProperty(ref _horizontalAlignment, value);
    }

    /// <summary>
    /// Gets or sets the text color of the label text.
    /// </summary>
    public Color TextColor
    {
        get => _textColor ?? _theme?.TextColor ?? DefaultTextColor;
        set => SetInvalidatingProperty(ref _textColor, value);
    }

    /// <summary>
    /// Gets or sets the background color of the label display control.
    /// </summary>
    public Color BackgroundColor
    {
        get => _backgroundColor ?? _theme?.BackgroundColor ?? DefaultBackgroundColor;
        set => SetInvalidatingProperty(ref _backgroundColor, value);
    }

    /// <summary>
    /// Gets or sets the text to be displayed on the label.
    /// </summary>
    public string Text
    {
        get => _text;
        set => SetInvalidatingProperty(ref _text, value);
    }

    /// <summary>
    /// Gets or sets the 2nd row of text to be displayed on the label.
    /// </summary>
    public string TextSecondary
    {
        get => _textSecondary;
        set => SetInvalidatingProperty(ref _textSecondary, value);
    }

    /// <summary>
    /// Gets or sets the font used for displaying the label text.
    /// </summary>
    public IFont? Font
    {
        get => _font;
        set => SetInvalidatingProperty(ref _font, value);
    }

    /// <summary>
    /// Gets or sets the font used for th displaying the 2nd row of label text.
    /// </summary>
    public IFont? FontSecondary
    {
        get => _fontSecondary;
        set => SetInvalidatingProperty(ref _fontSecondary, value);
    }

    /// <summary>
    /// ScaleFactor used to calculate drawn text size
    /// </summary>
    public ScaleFactor ScaleFactor
    {
        get => _scaleFactor;
        set => SetInvalidatingProperty(ref _scaleFactor, value);
    }

    private static Color DefaultTextColor = Color.White;
    private static Color DefaultBackgroundColor = Color.Transparent;

    private string _text = string.Empty;
    private string _textSecondary = string.Empty;

    private DisplayTheme? _theme;
    private Color? _textColor;
    private Color? _backgroundColor;
    private HorizontalAlignment _horizontalAlignment;
    private IFont? _font;
    private IFont? _fontSecondary;
    private ScaleFactor _scaleFactor = ScaleFactor.X1;

    /// <summary>
    /// Initializes a new instance of the <see cref="Label"/> class with the specified dimensions.
    /// </summary>
    /// <param name="width">The width of the label display control.</param>
    /// <param name="height">The height of the label display control.</param>
    /// <param name="text">The initial Text for the control</param>
    /// /<param name="textSecondary">The initial Text for the control</param>
    public DoubleLabel(int width, int height, string text = nameof(Label), string textSecondary = "")
        : this(0, 0, width, height, ScaleFactor.X1, text)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Label"/> class with the specified dimensions.
    /// </summary>
    /// <param name="width">The width of the label display control.</param>
    /// <param name="height">The height of the label display control.</param>
    /// <param name="scaleFactor">The scale factor used for drawing text</param>
    /// <param name="text">The initial Text for the control</param>
    /// <param name="textSecondary">The initial Text for the control</param>
    public DoubleLabel(int width, int height, ScaleFactor scaleFactor = ScaleFactor.X1, string text = nameof(Label), string textSecondary = "")
        : this(0, 0, width, height, scaleFactor, text, textSecondary)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Label"/> class with the specified dimensions.
    /// </summary>
    /// <param name="left">The left coordinate of the label display control.</param>
    /// <param name="top">The top coordinate of the label display control.</param>
    /// <param name="width">The width of the label display control.</param>
    /// <param name="height">The height of the label display control.</param>
    /// <param name="scaleFactor">The scale factor used for drawing text</param>
    /// <param name="text">The initial Text for the control</param>
    /// <param name="textSecondary">The initial Text for the control</param>
    public DoubleLabel(int left, int top, int width, int height, ScaleFactor scaleFactor = ScaleFactor.X1, string text = nameof(Label), string textSecondary = "")
        : base(left, top, width, height)
    {
        ScaleFactor = scaleFactor;
        Text = text;
        TextSecondary = textSecondary;
    }

    /// <summary>
    /// Applies the specified display theme to the label display control.
    /// </summary>
    /// <param name="theme">The display theme to apply.</param>
    public override void ApplyTheme(DisplayTheme theme)
    {
        _theme = theme;
        Invalidate();
    }

    /// <summary>
    /// Draws the label display control on the specified <see cref="MicroGraphics"/> surface.
    /// </summary>
    /// <param name="graphics">The <see cref="MicroGraphics"/> surface to draw the label display control on.</param>
    protected override void OnDraw(MicroGraphics graphics)
    {
        if (BackgroundColor != Color.Transparent)
        {
            graphics.DrawRectangle(ScreenLeft, ScreenTop, Width, Height, BackgroundColor, true);
        }

        var xOffset = HorizontalAlignment switch
        {
            HorizontalAlignment.Center => Width / 2,
            HorizontalAlignment.Right => Width,
            _ => 0,
        };
        var yOffset = Height / 4;

        graphics.DrawText(
            ScreenLeft + xOffset,
            ScreenTop + yOffset,
            Text,
            TextColor,
            scaleFactor: _scaleFactor,
            alignmentH: HorizontalAlignment,
            alignmentV: VerticalAlignment.Center,
            font: Font);

        yOffset = Height * 3 / 4;

        graphics.DrawText(
            ScreenLeft + xOffset,
            ScreenTop + yOffset,
            TextSecondary,
            TextColor,
            scaleFactor: _scaleFactor,
            alignmentH: HorizontalAlignment,
            alignmentV: VerticalAlignment.Center,
            font: FontSecondary);
    }
}
