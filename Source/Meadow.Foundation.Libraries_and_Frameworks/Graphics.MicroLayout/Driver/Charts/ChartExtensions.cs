using System;
using System.Collections.Generic;
using System.Linq;

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

    public static double MinX(this LineChartSeries series)
    {
        return (series.Points.Count == 0) ? 0 : series.Points.Min(p => p.X);
    }

    public static double MaxX(this LineChartSeries series)
    {
        return (series.Points.Count() == 0) ? 0 : series.Points.Max(p => p.X);
    }

    public static double MinY(this LineChartSeries series)
    {
        return (series.Points.Count() == 0) ? 0 : series.Points.Min(p => p.Y);
    }

    public static double MaxY(this LineChartSeries series)
    {
        return (series.Points.Count() == 0) ? 0 : series.Points.Max(p => p.Y);
    }

    public static double MinX(this IEnumerable<LineSeriesPoint> points)
    {
        return (points.Count() == 0) ? 0 : points.Min(p => p.X);
    }

    public static double MaxX(this IEnumerable<LineSeriesPoint> points)
    {
        return (points.Count() == 0) ? 0 : points.Max(p => p.X);
    }

    public static double MinY(this IEnumerable<LineSeriesPoint> points)
    {
        return (points.Count() == 0) ? 0 : points.Min(p => p.Y);
    }

    public static double MaxY(this IEnumerable<LineSeriesPoint> points)
    {
        return (points.Count() == 0) ? 0 : points.Max(p => p.Y);
    }
}
