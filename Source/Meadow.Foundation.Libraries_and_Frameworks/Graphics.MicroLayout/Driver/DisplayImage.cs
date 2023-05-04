using Meadow.Foundation;
using Meadow.Foundation.Graphics;

namespace MicroLayout;

public class DisplayImage : DisplayControl
{
    private Color _backColor = Color.Transparent;
    private VerticalAlignment _verticalAlignment = VerticalAlignment.Center;
    private HorizontalAlignment _horizontalAlignment;
    private Image _image;

    public DisplayImage(int left, int top, int width, int height, Image image)
        : base(left, top, width, height)
    {
        this.Image = image;
    }

    public override void ApplyTheme(DisplayTheme theme)
    {
        if (theme != null)
        {
            if (theme.BackgroundColor != null) this.BackColor = theme.BackgroundColor.Value;
        }
    }

    public Meadow.Foundation.Graphics.Image Image
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

        graphics.DrawImage(Left + (this.Width - Image.Width) / 2, Top + (this.Height - Image.Height) / 2, Image);
    }
}
