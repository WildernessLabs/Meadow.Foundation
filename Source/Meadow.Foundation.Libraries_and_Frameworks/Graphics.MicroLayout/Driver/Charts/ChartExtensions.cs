using System;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Extension methods for Chart objects
/// </summary>
public static class ChartExtensions
{
    /// <summary>
    /// Converts a 2-dimensional array of doubles to a LineCHartSeries
    /// </summary>
    /// <param name="xyData">the data in X,Y pairs</param>
    public static LineChartSeries ToLineChartSeries(this double[,] xyData)
    {
        if (xyData.Rank != 2) throw new ArgumentException("Expected a 2-dimensional array");

        var series = new LineChartSeries
        {
            LineStroke = 4,
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
