using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace HMI_Sample;

public class GridSampleLayout : StackLayout
{
    public GridSampleLayout(int width, int height)
        : base(0, 0, width, height)
    {
        this.BackgroundColor = Color.LightGray;

        var title = new Label(width, 22, text: "Grid Example")
        {
            Font = new Font12x20(),
            TextColor = Color.Black,
            BackgroundColor = Color.LightGray,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var testLabel1 = new Label(width, 22, "Test Label 1") { TextColor = Color.Red };
        var testLabel2 = new Label(width, 22, "Test Label 1") { TextColor = Color.Red };

        var grid = new GridLayout(0, 100, Width, height - 100, 5, 4)
        {
            BackgroundColor = Color.LightBlue,
            Padding = 5
        };
        for (var row = 0; row < 5; row++)
        {
            for (var col = 0; col < 4; col++)
            {
                var cell = new Label(0, 0, width / 4, 15, text: $"Cell {row * 4 + col + 1}")
                {
                    Font = new Font6x8(),
                    TextColor = Color.Black,
                    BackgroundColor = Color.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                grid.Add(cell, row, col);
            }
        }

        Controls.Add(title, testLabel1, testLabel2, grid);
    }
}
