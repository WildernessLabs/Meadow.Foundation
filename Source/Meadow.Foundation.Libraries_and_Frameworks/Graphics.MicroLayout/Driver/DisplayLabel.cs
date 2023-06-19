using Meadow.Foundation;
using Meadow.Foundation.Graphics;

namespace MicroLayout;

public class DisplayLabel : DisplayControl
{
    private string _text;

    private Color _textColor = Color.White;
    private Color _backColor = Color.Transparent;
    private VerticalAlignment _verticalAlignment = VerticalAlignment.Center;
    private HorizontalAlignment _horizontalAlignment;
    private IFont? _font;

    public DisplayLabel(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
    }

    public override void ApplyTheme(DisplayTheme theme)
    {
        if (theme != null)
        {
            if (theme.TextColor != null) this.TextColor = theme.TextColor.Value;
            if (theme.BackgroundColor != null) this.BackColor = theme.BackgroundColor.Value;
            this.Font = theme.Font;
        }
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

    public Color TextColor
    {
        get => _textColor;
        set => SetInvalidatingProperty(ref _textColor, value);
    }

    public Color BackColor
    {
        get => _backColor;
        set => SetInvalidatingProperty(ref _backColor, value);
    }

    public string Text
    {
        get => _text;
        set => SetInvalidatingProperty(ref _text, value);
    }

    public IFont? Font
    {
        get => _font;
        set => SetInvalidatingProperty(ref _font, value);
    }

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
                x = this.Width / 2;
                break;
            case HorizontalAlignment.Right:
                x = this.Width;
                break;
            default:
                x = 0;
                break;
        }
        switch (VerticalAlignment)
        {
            case VerticalAlignment.Center:
                y = this.Height / 2;
                break;
            case VerticalAlignment.Bottom:
                y = this.Height;
                break;
            default:
                y = 0;
                break;
        }

        graphics.DrawRectangle(Left, Top, Width, Height, BackColor, true);
        graphics.DrawText(Left + x, Top + y, Text, TextColor, alignmentH: HorizontalAlignment, alignmentV: VerticalAlignment, font: Font);
    }
}
