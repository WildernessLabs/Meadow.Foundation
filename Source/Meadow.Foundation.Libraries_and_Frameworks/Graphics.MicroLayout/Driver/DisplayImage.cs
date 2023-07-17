using Meadow.Foundation.Graphics;

namespace Meadow.Foundation.Displays.UI;

public class DisplayImage : DisplayControl
{
    private Color _backColor = Color.Transparent;
    private VerticalAlignment _verticalAlignment = VerticalAlignment.Center;
    private HorizontalAlignment _horizontalAlignment;
    private Image _image;

    public DisplayImage(int left, int top, int width, int height, Image image)
        : base(left, top, width, height)
    {
        Image = image;
    }

    public override void ApplyTheme(DisplayTheme theme)
    {
        if (theme != null)
        {
            if (theme.BackgroundColor != null) BackColor = theme.BackgroundColor.Value;
        }
    }

    public Image Image
    {
        get => _image;
        set => SetInvalidatingProperty(ref _image, value);
    }

    public VerticalAlignment VerticalAlignment
    {
        get => _verticalAlignment;
        set => SetInvalidatingProperty(ref _verticalAlignment, value);
    }

    public HorizontalAlignment HorizontalAlignment
    {
        get => _horizontalAlignment;
        set => SetInvalidatingProperty(ref _horizontalAlignment, value);
    }

    public Color BackColor
    {
        get => _backColor;
        set => SetInvalidatingProperty(ref _backColor, value);
    }

    protected override void OnDraw(MicroGraphics graphics)
    {
        if (BackColor != Color.Transparent)
        {
            graphics.DrawRectangle(Left, Top, Width, Height, BackColor, true);
        }

        graphics.DrawImage(Left + (Width - Image.Width) / 2, Top + (Height - Image.Height) / 2, Image);
    }
}
