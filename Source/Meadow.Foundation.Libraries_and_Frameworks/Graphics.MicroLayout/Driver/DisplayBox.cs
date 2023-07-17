using Meadow.Foundation.Graphics;

namespace Meadow.Foundation.Displays.UI;

public class DisplayBox : DisplayControl
{
    private Color _foreColor;

    public bool Filled { get; set; } = true;

    public DisplayBox(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
    }

    public override void ApplyTheme(DisplayTheme theme)
    {
        if (theme != null)
        {
            if (theme.ForegroundColor != null) this.ForeColor = theme.ForegroundColor.Value;
        }
    }

    public Color ForeColor
    {
        get => _foreColor;
        set => SetInvalidatingProperty(ref _foreColor, value);
    }

    protected override void OnDraw(MicroGraphics graphics)
    {
        if (ForeColor != Color.Transparent)
        {
            graphics.DrawRectangle(Left, Top, Width, Height, ForeColor, Filled);
        }
    }
}
