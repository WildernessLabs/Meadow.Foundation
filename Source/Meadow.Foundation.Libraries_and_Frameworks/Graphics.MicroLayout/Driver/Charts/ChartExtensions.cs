using System;

namespace Meadow.Foundation.Graphics.MicroLayout;

public static class ChartExtensions
{
    public static LineChartSeries ToLineChartSeries(this double[,] xyData)
    {
        if (xyData.Rank != 2) throw new ArgumentException("Expected a 2-dimensional array");

        var series = new LineChartSeries
        {
            LineWidth = 4,
            LineColor = Color.Green,
            ShowLines = true,
            PointSize = 5,
            PointColor = Color.Yellow,
            ShowPoints = true
        };

        var points = xyData.GetLength(0);

        for (var i = 0; i < points; i++)
        {
            series.Points.Add(new LineSeriesPoint(xyData[i, 0], xyData[i, 1]));
        }

        return series;
    }

}
