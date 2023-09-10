namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a label display control in the user interface.
/// </summary>
public class DisplayLabel : ThemedControl
{
    private string _text;

    private Color _textColor = Color.White;
    private Color _backColor = Color.Transparent;
    private VerticalAlignment _verticalAlignment = VerticalAlignment.Center;
    private HorizontalAlignment _horizontalAlignment;
    private IFont? _font;
    private ScaleFactor _scaleFactor = ScaleFactor.X1;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisplayLabel"/> class with the specified dimensions.
    /// </summary>
    /// <param name="left">The left coordinate of the label display control.</param>
    /// <param name="top">The top coordinate of the label display control.</param>
    /// <param name="width">The width of the label display control.</param>
    /// <param name="height">The height of the label display control.</param>
    /// <param name="scaleFactor">The scale factor used for drawing text</param>
    public DisplayLabel(int left, int top, int width, int height, ScaleFactor scaleFactor = ScaleFactor.X1)
        : base(left, top, width, height)
    {
        ScaleFactor = scaleFactor;
    }

    /// <summary>
    /// Applies the specified display theme to the label display control.
    /// </summary>
    /// <param name="theme">The display theme to apply.</param>
    public override void ApplyTheme(DisplayTheme theme)
    {
        if (theme != null)
        {
            if (theme.TextColor != null) TextColor = theme.TextColor.Value;
            if (theme.BackgroundColor != null) BackColor = theme.BackgroundColor.Value;

            if (Font == null)
            {
                Font = theme.Font;
            }
        }
    }

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
        get => _textColor;
        set => SetInvalidatingProperty(ref _textColor, value);
    }

    /// <summary>
    /// Gets or sets the background color of the label display control.
    /// </summary>
    public Color BackColor
    {
        get => _backColor;
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

    /// <summary>
    /// Draws the label display control on the specified <see cref="MicroGraphics"/> surface.
    /// </summary>
    /// <param name="graphics">The <see cref="MicroGraphics"/> surface to draw the label display control on.</param>
    protected override void OnDraw(MicroGraphics graphics)
    {
        if (BackColor != Color.Transparent)
        {
            graphics.DrawRectangle(Left, Top, Width, Height, BackColor, true);
        }

        int x, y;

        switch (HorizontalAlignment)
        {
            case HorizontalAlignment.Center:
                x = Width / 2;
                break;
            case HorizontalAlignment.Right:
                x = Width;
                break;
            default:
                x = 0;
                break;
        }
        switch (VerticalAlignment)
        {
            case VerticalAlignment.Center:
                y = Height / 2;
                break;
            case VerticalAlignment.Bottom:
                y = Height;
                break;
            default:
                y = 0;
                break;
        }

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
