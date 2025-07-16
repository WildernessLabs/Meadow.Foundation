namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a label display control in the user interface.
/// </summary>
public class Label : ClickableControl
{
    /// <summary>
    /// Gets or sets the vertical alignment of the label text within the label display control.
    /// </summary>
    public VerticalAlignment VerticalAlignment
    {
        get => _verticalAlignment;
        set => SetInvalidatingProperty(ref _verticalAlignment, value);
    }

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
        get => _backColor ?? _theme?.BackgroundColor ?? DefaultBackColor;
        set => SetInvalidatingProperty(ref _backColor, value);
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
    /// Gets or sets the font used for displaying the label text.
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

    private static Color DefaultTextColor = Color.White;
    private static Color DefaultBackColor = Color.Transparent;

    private string _text = string.Empty;

    private DisplayTheme? _theme;
    private Color? _textColor;
    private Color? _backColor;
    private VerticalAlignment _verticalAlignment = VerticalAlignment.Center;
    private HorizontalAlignment _horizontalAlignment;
    private IFont? _font;
    private ScaleFactor _scaleFactor = ScaleFactor.X1;

    /// <summary>
    /// Initializes a new instance of the <see cref="Label"/> class with the specified dimensions.
    /// </summary>
    /// <param name="width">The width of the label display control.</param>
    /// <param name="height">The height of the label display control.</param>
    /// <param name="text">The initial Text for the control</param>
    public Label(int width, int height, string text = nameof(Label))
        : this(0, 0, width, height, ScaleFactor.X1, text)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Label"/> class with the specified dimensions.
    /// </summary>
    /// <param name="width">The width of the label display control.</param>
    /// <param name="height">The height of the label display control.</param>
    /// <param name="scaleFactor">The scale factor used for drawing text</param>
    /// <param name="text">The initial Text for the control</param>
    public Label(int width, int height, ScaleFactor scaleFactor = ScaleFactor.X1, string text = nameof(Label))
        : this(0, 0, width, height, scaleFactor, text)
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
    public Label(int left, int top, int width, int height, ScaleFactor scaleFactor = ScaleFactor.X1, string text = nameof(Label))
        : base(left, top, width, height)
    {
        ScaleFactor = scaleFactor;
        Text = text;
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

        var x = HorizontalAlignment switch
        {
            HorizontalAlignment.Center => Width / 2,
            HorizontalAlignment.Right => Width,
            _ => 0,
        };
        var y = VerticalAlignment switch
        {
            VerticalAlignment.Center => Height / 2,
            VerticalAlignment.Bottom => Height,
            _ => 0,
        };

        x += Parent?.Left ?? 0;
        y += Parent?.Top ?? 0;

        graphics.DrawText(
            Left + x,
            Top + y,
            Text,
            TextColor,
            scaleFactor: _scaleFactor,
            alignmentH: HorizontalAlignment,
            alignmentV: VerticalAlignment,
            font: Font);
    }
}
