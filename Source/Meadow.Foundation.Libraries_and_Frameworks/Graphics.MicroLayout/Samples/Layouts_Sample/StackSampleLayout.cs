using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace HMI_Sample;

public class StackSampleLayout : StackLayout
{
    public StackSampleLayout(int width, int height)
        : base(0, 0, width, height, Orientation.Vertical)
    {
        var title = new Label(width, 22, text: "StackLayout Example")
        {
            Font = new Font12x16(),
            TextColor = Color.Black,
            BackgroundColor = Color.LightGray,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var horizontalStack = new StackLayout(
            20, 0,
            width - 40, 30,
            Orientation.Horizontal,
            CrossAxisAlignment.Center);
        horizontalStack.BackgroundColor = Color.DarkGray;

        var labelH1 = new Label(80, 20, "Item 1")
        {
            Font = new Font8x12(),
            TextColor = Color.White,
            BackgroundColor = Color.DarkGreen,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        var labelH2 = new Label(100, 25, "Item 2")
        {
            Font = new Font8x12(),
            TextColor = Color.White,
            BackgroundColor = Color.DarkBlue,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        var labelH3 = new Label(70, 20, "Item 3")
        {
            Font = new Font8x12(),
            TextColor = Color.White,
            BackgroundColor = Color.DarkRed,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        horizontalStack.Controls.Add(labelH1, labelH2, labelH3);

        var labelV1 = new Label(width, 25, "Item 1")
        {
            Font = new Font8x12(),
            TextColor = Color.White,
            BackgroundColor = Color.Purple,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var labelV2 = new Label(150, 30, "Item 2")
        {
            Font = new Font8x12(),
            TextColor = Color.Black,
            BackgroundColor = Color.Orange,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var labelV3 = new Label(100, 40, "Item 3")
        {
            Font = new Font8x12(),
            TextColor = Color.Black,
            BackgroundColor = Color.Yellow,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var labelV4 = new Label(100, 50, "Item 4")
        {
            Font = new Font8x12(),
            TextColor = Color.White,
            BackgroundColor = Color.Brown,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var verticalItemsLayout = new StackLayout(
            0, 0,
            width, height - title.Height - horizontalStack.Height,
            Orientation.Vertical,
            CrossAxisAlignment.Center);

        verticalItemsLayout.BackgroundColor = Color.DarkGreen;

        verticalItemsLayout.Controls.Add(labelV1);
        verticalItemsLayout.Controls.Add(labelV2);
        verticalItemsLayout.Controls.Add(labelV3);
        verticalItemsLayout.Controls.Add(labelV4);


        Controls.Add(title);
        Controls.Add(horizontalStack);
        Controls.Add(verticalItemsLayout);

        Padding = 5;
        Spacing = 5;
        BackgroundColor = Color.DimGray;
    }
}