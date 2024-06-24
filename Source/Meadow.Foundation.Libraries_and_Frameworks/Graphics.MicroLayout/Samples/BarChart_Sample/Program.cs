using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace Meadow.Foundation.Displays.UI;

public class MeadowApp : App<Desktop>
{
    private DisplayScreen? screen;

    public static async Task Main(string[] args)
    {
        await MeadowOS.Start(args);
    }

    public override Task Run()
    {
        var labelFont = new Font12x20();

        screen = new DisplayScreen(Device.Display);
        screen.BackgroundColor = Color.AntiqueWhite;

        var random = new Random();
        var seriesA = new float[10];
        for (var i = 0; i < seriesA.Length; i++)
        {
            seriesA[i] = random.NextSingle() * 10;
        }
        var seriesB = new float[12];
        for (var i = 0; i < seriesB.Length; i++)
        {
            seriesB[i] = random.NextSingle() * 10;
        }
        var seriesC = new float[23];
        for (var i = 0; i < seriesC.Length; i++)
        {
            seriesC[i] = random.NextSingle() * 10;
        }

        var chartA = new VerticalBarChart(0, 0, screen.Width / 2, screen.Height / 2, seriesA)
        {
            AxisLabelColor = Color.Red,
            AxisColor = Color.DarkRed,
            AxisStroke = 3,
            BarSpacing = 3,
            SeriesColor = Color.DarkBlue,
            AxisFont = new Font8x8(),
        };
        var chartB = new VerticalBarChart(0, screen.Height / 2, screen.Width, screen.Height / 2, seriesB)
        {
            AxisLabelColor = Color.Yellow,
            AxisColor = Color.DarkRed,
            AxisStroke = 3,
            BarSpacing = 3,
            SeriesColor = Color.DarkGreen,
            AxisFont = new Font12x20(),
            XAxisLabelFormat = "N2"
        };
        var chartC = new VerticalBarChart(screen.Width / 2, 0, screen.Width / 2, screen.Height / 2, seriesC)
        {
            AxisLabelColor = Color.Orange,
            AxisColor = Color.Magenta,
            AxisStroke = 2,
            BarSpacing = 2,
            SeriesColor = Color.Purple,
            AxisFont = new Font12x20(),
            ShowXAxisLabels = false
        };

        screen.Controls.Add(chartA, chartB, chartC);

        return Task.CompletedTask;
    }
}