using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace HMI_Sample;

public class AlignmentSampleLayout : AlignmentLayout
{
    public AlignmentSampleLayout(int width, int height)
        : base(0, 0, width, height)
    {
        BackgroundColor = Color.DarkGray;
        Padding = 10;

        var centerLabel = new Label(100, 50, "CENTER")
        {
            Font = new Font8x12(),
            TextColor = Color.White,
            BackgroundColor = Color.Red,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Add(centerLabel, DockPosition.Center);

        var topLabel = new Label(150, 25, "TOP")
        {
            Font = new Font8x12(),
            TextColor = Color.Black,
            BackgroundColor = Color.LightBlue,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Add(topLabel, DockPosition.Top);

        var bottomLabel = new Label(150, 25, "BOTTOM")
        {
            Font = new Font8x12(),
            TextColor = Color.White,
            BackgroundColor = Color.DarkBlue,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Add(bottomLabel, DockPosition.Bottom);

        var leftLabel = new Label(50, 80, "LEFT")
        {
            Font = new Font8x12(),
            TextColor = Color.Black,
            BackgroundColor = Color.LightGreen,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Add(leftLabel, DockPosition.Left);

        var rightLabel = new Label(50, 80, "RIGHT")
        {
            Font = new Font8x12(),
            TextColor = Color.White,
            BackgroundColor = Color.DarkGreen,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Add(rightLabel, DockPosition.Right);

        var topLeftLabel = new Label(60, 20, "TL")
        {
            Font = new Font6x8(),
            TextColor = Color.Black,
            BackgroundColor = Color.Yellow,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Add(topLeftLabel, DockPosition.TopLeft);

        var topRightLabel = new Label(60, 20, "TR")
        {
            Font = new Font6x8(),
            TextColor = Color.Black,
            BackgroundColor = Color.Orange,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Add(topRightLabel, DockPosition.TopRight);

        var bottomLeftLabel = new Label(60, 20, "BL")
        {
            Font = new Font6x8(),
            TextColor = Color.Black,
            BackgroundColor = Color.Magenta,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Add(bottomLeftLabel, DockPosition.BottomLeft);

        var bottomRightLabel = new Label(60, 20, "BR")
        {
            Font = new Font6x8(),
            TextColor = Color.Black,
            BackgroundColor = Color.Cyan,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Add(bottomRightLabel, DockPosition.BottomRight);
    }
}