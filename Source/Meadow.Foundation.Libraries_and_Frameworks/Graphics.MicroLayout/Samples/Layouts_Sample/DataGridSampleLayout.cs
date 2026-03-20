using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace HMI_Sample;

public class DataGridSampleLayout : AbsoluteLayout
{
    public DataGridSampleLayout(int width, int height)
        : base(0, 0, width, height)
    {
        this.BackgroundColor = Color.LightGray;

        var title = new Label(0, 0, width, 22, text: "DataGrid Example")
        {
            Font = new Font12x20(),
            TextColor = Color.Black,
            BackgroundColor = Color.LightGray,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        // Create a scrollable text layout with the specified dimensions
        var grid = new DataGrid(0, title.Bottom, width, Height,
            new[]
            {
                new DataGrid.ColumnDefinition(30, "A") { HorizontalAlignment = HorizontalAlignment.Left },
                new DataGrid.ColumnDefinition(40, "B"),
                new DataGrid.ColumnDefinition(50, "C"),
                new DataGrid.ColumnDefinition(30, "D"),
                new DataGrid.ColumnDefinition(100, "E") { HorizontalAlignment = HorizontalAlignment.Center },
            })
        {
            EvenRowBackgroundColor = Color.LightBlue,
            EvenRowTextColor = Color.Black,
        };

        grid.AddRow("1", "Two", "Three", "4", "Five");
        grid.AddRow("6", "Seven", "Eight", "9", "Ten");
        grid.AddRow("11", "Twelve", "Thirteen", "14", "Fifteen");
        grid.AddRow("16", "Seventeen", "Eighteen", "19", "Twenty");

        this.Controls.Add(title, grid);
    }
}
