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

        var splashLayout = new AbsoluteLayout(0, 0, screen.Width, screen.Height)
        {
        };

        var logo = Image.LoadFromFile("logo.bmp");
        var wlLabel = new Graphics.MicroLayout.Label(0, 10, screen.Width, 32)
        {
            TextColor = Color.White,
            BackColor = Color.DarkRed,
            Text = "Hello Meadow"
        };
        var logoImg = new Graphics.MicroLayout.Picture(
                0, 0,
                screen.Width,
                screen.Height,
                logo
            );
        splashLayout.Controls.Add(logoImg, wlLabel);

        var chartLayout = new AbsoluteLayout(0, 0, screen.Width, screen.Height);

        var chart1Label = new Graphics.MicroLayout.Label(0, 0, screen.Width, 16)
        {
            Text = "Values for process A",
            TextColor = Color.Aquamarine,
            BackColor = Color.Black,
            Font = labelFont,
        };

        var chart1 = new LineChart(0, 16, screen.Width, (screen.Height / 2) - 16)
        {
            BackgroundColor = Color.FromHex("111111"),
            ShowYAxisLabels = true
        };

        chart1.Series.Add(
            GetSineSeries(),
            GetCosineSeries(4, 4.2, 0));

        var chart2Label = new Graphics.MicroLayout.Label(0, screen.Height / 2, screen.Width, 16)
        {
            Text = "Values for process B",
            TextColor = Color.BlueViolet,
            BackColor = Color.Black,
            Font = labelFont,
        };

        var chart2 = new LineChart(0, (screen.Height / 2) + 16, screen.Width, (screen.Height / 2) - 16)
        {
            BackgroundColor = Color.FromHex("222222"),
            ShowYAxisLabels = true
        };

        chart2.Series.Add(
            GetSineSeries(2, 2),
            GetCosineSeries(4, 4.2, 4.5));

        chartLayout.Controls.Add(chart1Label, chart2Label, chart1, chart2);

        screen.Controls.Add(splashLayout, chartLayout);

        chartLayout.IsVisible = false;

        Task.Run(async () =>
        {
            await Task.Delay(5000);
            splashLayout.IsVisible = false;
            chartLayout.IsVisible = true;
        });

        return base.Run();
    }

    private const int PointsPerSeries = 50;

    private LineChartSeries GetSineSeries(double xScale = 4, double yScale = 1.5, double yOffset = 1.5)
    {
        var series = new LineChartSeries()
        {
            LineColor = Color.Red,
            PointColor = Color.Green,
            LineStroke = 4,
            PointSize = 6,
            ShowLines = true,
            ShowPoints = true,

        };

        for (var p = 0; p < PointsPerSeries; p++)
        {
            series.Points.Add(p * 2, (Math.Sin(p / xScale) * yScale) + yOffset);
        }

        return series;
    }

    private LineChartSeries GetCosineSeries(double xScale = 4, double yScale = 1.5, double yOffset = 4.5)
    {
        var series = new LineChartSeries()
        {
            LineColor = Color.DarkBlue,
            PointColor = Color.DarkGreen,
            LineStroke = 4,
            PointSize = 6,
            ShowLines = true,
            ShowPoints = true,

        };

        for (var p = 0; p < PointsPerSeries; p++)
        {
            series.Points.Add(p * 2, (Math.Cos(p / xScale) * yScale) + yOffset);
        }

        return series;
    }
}