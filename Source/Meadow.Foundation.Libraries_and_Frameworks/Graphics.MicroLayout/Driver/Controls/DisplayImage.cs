namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents an image display control in the user interface.
/// </summary>
public class DisplayImage : ThemedDisplayControl
{
    private Color _backColor = Color.Transparent;
    private VerticalAlignment _verticalAlignment = VerticalAlignment.Center;
    private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Center;
    private Image _image = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisplayImage"/> class with the specified dimensions and image.
    /// </summary>
    /// <param name="left">The left coordinate of the image display control.</param>
    /// <param name="top">The top coordinate of the image display control.</param>
    /// <param name="width">The width of the image display control.</param>
    /// <param name="height">The height of the image display control.</param>
    /// <param name="image">The image to be displayed.</param>
    public DisplayImage(int left, int top, int width, int height, Image image)
        : base(left, top, width, height)
    {
        Image = image;
    }

    /// <summary>
    /// Applies the specified display theme to the image display control.
    /// </summary>
    /// <param name="theme">The display theme to apply.</param>
    public override void ApplyTheme(DisplayTheme theme)
    {
        if (theme != null)
        {
            if (theme.BackgroundColor != null) BackColor = theme.BackgroundColor.Value;
        }
    }

    /// <summary>
    /// Gets or sets the image to be displayed on the image display control.
    /// </summary>
    public Image Image
    {
        get => _image;
        set => SetInvalidatingProperty(ref _image, value);
    }

    /// <summary>
    /// Gets or sets the vertical alignment of the image within the image display control.
    /// </summary>
    public VerticalAlignment VerticalAlignment
    {
        get => _verticalAlignment;
        set => SetInvalidatingProperty(ref _verticalAlignment, value);
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of the image within the image display control.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment
    {
        get => _horizontalAlignment;
        set => SetInvalidatingProperty(ref _horizontalAlignment, value);
    }

    /// <summary>
    /// Gets or sets the background color of the image display control.
    /// </summary>
    public Color BackColor
    {
        get => _backColor;
        set => SetInvalidatingProperty(ref _backColor, value);
    }

    /// <summary>
    /// Draws the image display control on the specified <see cref="MicroGraphics"/> surface.
    /// </summary>
    /// <param name="graphics">The <see cref="MicroGraphics"/> surface to draw the image display control on.</param>
    protected override void OnDraw(MicroGraphics graphics)
    {
        if (BackColor != Color.Transparent)
        {
            graphics.DrawRectangle(Left, Top, Width, Height, BackColor, true);
        }

        int x, y;
        if (HorizontalAlignment == HorizontalAlignment.Center)
        {
            x = Left + ((Width - Image.Width) / 2);
        }
        else if (HorizontalAlignment == HorizontalAlignment.Right)
        {
            x = Right - Image.Width;
        }
        else // Default to Left alignment
        {
            x = Left;
        }

        if (VerticalAlignment == VerticalAlignment.Center)
        {
            y = Top + ((Height - Image.Height) / 2);
        }
        else if (VerticalAlignment == VerticalAlignment.Bottom)
        {
            y = Bottom - Image.Height;
        }
        else // Default to Top alignment
        {
            y = Top;
        }

        graphics.DrawImage(x, y, Image);
    }
}
