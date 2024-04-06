using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace Meadow.Foundation.Displays.UI;

public class MeadowApp : App<Windows>
{
    private DisplayScreen? screen;

    public static async Task Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        await MeadowOS.Start(args);
    }

    public override Task Run()
    {
        var display = new WinFormsDisplay();
        display.ControlBox = true;

        var labelFont = new Font12x20();

        screen = new DisplayScreen(display);
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

        var chartA = new VerticalBarChart(0, 0, screen.Width, screen.Height / 2, seriesA)
        {
            AxisLabelColor = Color.Red,
            AxisColor = Color.DarkRed,
            AxisStroke = 3,
            BarSpacing = 3,
            SeriesColor = Color.DarkBlue,
            AxisFont = new Font12x20(),
        };
        var chartB = new VerticalBarChart(0, screen.Height / 2, screen.Width, screen.Height / 2, seriesB)
        {
            AxisLabelColor = Color.Yellow,
            AxisColor = Color.DarkRed,
            AxisStroke = 3,
            BarSpacing = 3,
            SeriesColor = Color.DarkGreen,
            AxisFont = new Font12x20(),
        };


        screen.Controls.Add(chartA, chartB);

        System.Windows.Forms.Application.Run(display);

        return Task.CompletedTask;
    }
}