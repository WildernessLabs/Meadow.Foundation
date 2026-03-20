using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace HMI_Sample;

public class ScrollableTextLayout : AbsoluteLayout
{
    public ScrollableTextLayout(int width, int height)
        : base(0, 0, width, height)
    {
        this.BackgroundColor = Color.LightGray;

        var title = new Label(0, 0, width, 22, text: "Scrollable Text Example")
        {
            Font = new Font12x20(),
            TextColor = Color.Black,
            BackgroundColor = Color.LightGray,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // Create a scrollable text layout with the specified dimensions
        var scrollableText = new ScrollingTextArea(0, Height / 2, width, Height / 2, new Font6x8()) { BackgroundColor = Color.LightGray };

        for (var i = 0; i < scrollableText.RowCount; i++)
        {
            scrollableText.Add($"Line {i + 1}");
        }

        Controls.Add(title, scrollableText);
    }

    protected override void OnDraw(MicroGraphics graphics)
    {
        base.OnDraw(graphics);
    }
}
