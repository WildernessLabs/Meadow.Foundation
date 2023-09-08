using Meadow.Foundation.Graphics.MicroLayout;
using System.Windows.Forms;

namespace Meadow.Foundation.Displays.UI;

public class MeadowApp : App<Windows>
{
    private DisplayScreen _screen;

    public static async Task Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        await MeadowOS.Start(args);
    }

    public override Task Run()
    {
        var display = new WinFormsDisplay();
        display.ControlBox = true;

        _screen = new DisplayScreen(display);
        _screen.BackgroundColor = Color.AntiqueWhite;

        var chart1 = new DisplayLineChart(0, 0, _screen.Width, _screen.Height / 2)
        {
            BackgroundColor = Color.FromHex("111111"),
            ShowYAxisLabels = true
        };

        chart1.Series.Add(
            GetSineSeries(),
            GetCosineSeries(4, 4.2, 0));

        var chart2 = new DisplayLineChart(0, _screen.Height / 2, _screen.Width, _screen.Height / 2)
        {
            BackgroundColor = Color.FromHex("222222"),
            ShowYAxisLabels = true
        };

        chart2.Series.Add(
            GetSineSeries(2, 2),
            GetCosineSeries(4, 4.2, 4.5));

        _screen.Controls.Add(chart1, chart2);

        chart1.Invalidate();

        Application.Run(display);

        return base.Run();
    }

    private const int PointsPerSeries = 50;

    private LineChartSeries GetSineSeries(double xScale = 4, double yScale = 1.5, double yOffset = 1.5)
    {
        var series = new LineChartSeries()
        {
            LineColor = Color.Red,
            PointColor = Color.Green,
            LineWidth = 4,
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
            LineWidth = 4,
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
