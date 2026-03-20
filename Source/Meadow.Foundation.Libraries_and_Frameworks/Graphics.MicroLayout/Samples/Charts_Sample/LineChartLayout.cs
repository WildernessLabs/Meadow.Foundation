using Meadow;
using Meadow.Foundation.Graphics.MicroLayout;

namespace Charts_Sample;

public class LineChartLayout : AbsoluteLayout
{
    private const int PointsPerSeries = 50;

    public LineChartLayout(int width, int height)
        : base(0, 0, width, height)
    {
        var chart = new LineChart(0, 0, width, height)
        {
            BackgroundColor = Color.Black,
            ShowYAxisLabels = true,
            AxisStroke = 1,
            AxisColor = Color.Gray,
        };

        chart.Series.Add(
            GetSineSeries(),
            GetCosineSeries(4, 4.2, 0));

        Controls.Add(chart);

        BackgroundColor = Color.DimGray;
    }

    private LineChartSeries GetSineSeries(double xScale = 4, double yScale = 1.5, double yOffset = 1.5)
    {
        var series = new LineChartSeries()
        {
            LineColor = Color.DarkGreen,
            PointColor = Color.Green,
            LineStroke = 1,
            PointSize = 5,
            ShowLines = true,
            ShowPoints = false,
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
            PointColor = Color.Blue,
            LineStroke = 1,
            PointSize = 5,
            ShowLines = true,
            ShowPoints = false,

        };

        for (var p = 0; p < PointsPerSeries; p++)
        {
            series.Points.Add(p * 2, (Math.Cos(p / xScale) * yScale) + yOffset);
        }

        return series;
    }
}