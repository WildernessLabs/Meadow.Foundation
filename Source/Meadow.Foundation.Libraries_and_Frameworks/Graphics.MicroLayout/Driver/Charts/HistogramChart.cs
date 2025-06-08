using System;
using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a histogram chart control.
/// </summary>
public class HistogramChart : ChartControl
{
    private List<HistogramChartSeries> _series = new();
    private bool _showXLabels = true;
    private int? _maxXAxisValue = null;
    private int? _minXAxisValue = null;
    private int? _maxYAxisValue = null;

    /// <summary>
    /// Creates a vertical bar chart instance
    /// </summary>
    /// <param name="left">The control's left position</param>
    /// <param name="top">The control's top position</param>
    /// <param name="width">The control's width</param>
    /// <param name="height">The control's height</param>
    public HistogramChart(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
    }

    /// <summary>
    /// Creates a vertical bar chart instance
    /// </summary>
    /// <param name="left">The control's left position</param>
    /// <param name="top">The control's top position</param>
    /// <param name="width">The control's width</param>
    /// <param name="height">The control's height</param>
    /// <param name="series">A series of data to plot</param>
    public HistogramChart(int left, int top, int width, int height, IEnumerable<HistogramChartSeries> series)
        : base(left, top, width, height)
    {
        Series = series.ToList();
    }

    /// <summary>
    /// Gets or sets a series of float values to plot
    /// </summary>
    public List<HistogramChartSeries> Series
    {
        get => _series;
        set => SetInvalidatingProperty(ref _series, value);
    }

    /// <summary>
    /// Gets or sets the visibility of the X axis labels
    /// </summary>
    public bool ShowXAxisLabels
    {
        get => _showXLabels;
        set => SetInvalidatingProperty(ref _showXLabels, value);
    }

    /// <summary>
    /// Gets or sets an optional minimum X Axis value
    /// </summary>
    public int? MinXAxisValue
    {
        get => _minXAxisValue;
        set => SetInvalidatingProperty(ref _minXAxisValue, value);
    }

    /// <summary>
    /// Gets or sets an optional maximum X Axis value
    /// </summary>
    public int? MaxXAxisValue
    {
        get => _maxXAxisValue;
        set => SetInvalidatingProperty(ref _maxXAxisValue, value);
    }

    /// <summary>
    /// Gets or sets an optional maximum Y Axis value
    /// </summary>
    public int? MaxYAxisValue
    {
        get => _maxYAxisValue;
        set => SetInvalidatingProperty(ref _maxYAxisValue, value);
    }

    /// <inheritdoc/>
    protected override void OnDraw(MicroGraphics graphics)
    {
        graphics.DrawRectangle(Left, Top, Width, Height, BackgroundColor, true);

        var font = GetAxisFont();

        ChartAreaTop = Top + (DefaultMargin * 2) - AxisStroke;
        ChartAreaBottom = Bottom - DefaultMargin - AxisStroke;
        if (ShowXAxisLabels)
        {
            ChartAreaBottom -= font.Height;
        }
        ChartAreaHeight = Height - (DefaultMargin * 3);

        ChartAreaLeft = Left + DefaultMargin;

        ChartAreaWidth = Width - DefaultMargin;

        DrawSeries(graphics, Series, font);

        graphics.DrawRectangle(
            ChartAreaLeft,
            ChartAreaBottom,
            ChartAreaWidth - DefaultMargin,
            AxisStroke,
            color: AxisColor,
            filled: true);
    }

    /// <summary>
    /// Called to draw a value bar in the histogram chart.
    /// </summary>
    /// <param name="graphics">The graphics context used to draw the bar.</param>
    /// <param name="seriesNumber">The series number to which the bar belongs.</param>
    /// <param name="value">The value represented by the bar.</param>
    /// <param name="x">The X-coordinate of the top-left corner of the bar.</param>
    /// <param name="y">The Y-coordinate of the top-left corner of the bar.</param>
    /// <param name="width">The width of the bar.</param>
    /// <param name="height">The height of the bar.</param>
    /// <param name="barColor">The color of the bar.</param>
    /// <param name="filled">A value indicating whether the bar is filled or just outlined.</param>
    protected virtual void DrawValueBar(
        MicroGraphics graphics,
        int seriesNumber,
        (int X, int Y) value,
        int x,
        int y,
        int width,
        int height,
        Color barColor,
        bool filled)
    {
        graphics.DrawRectangle(x, y, width, height, barColor, filled);
    }

    private void DrawSeries(MicroGraphics graphics, List<HistogramChartSeries> seriesList, IFont font)
    {
        var barWidth = 3;
        var halfWidth = barWidth / 2;

        var count = seriesList.SelectMany(e => e.DataElements).Count();
        if (count == 0) return;

        var minX = MinXAxisValue ?? seriesList.SelectMany(e => e.DataElements).Min(x => x.X);
        var maxX = MaxXAxisValue ?? seriesList.SelectMany(e => e.DataElements).Max(x => x.X);
        int maxY = MaxYAxisValue ?? seriesList.SelectMany(e => e.DataElements).Max(y => y.Y);

        var heightScale = ChartAreaHeight * 0.9f / maxY;

        for (var s = seriesList.Count - 1; s >= 0; s--)
        {
            if (seriesList[s].DataElements.Count() == 0) continue;

            foreach (var pair in seriesList[s].DataElements)
            {
                // Normalize the value to the range [0, 1]
                double normalizedValue = (Math.Log(pair.X) - Math.Log(minX)) / (Math.Log(maxX) - Math.Log(minX));

                // Map the normalized value to the display width
                int x = (int)(normalizedValue * ChartAreaWidth) + ChartAreaLeft;

                var barHeight = (int)(heightScale * pair.Y);

                // make sure we don't draw off-chart for over-scale values
                if (barHeight > ChartAreaHeight)
                {
                    barHeight = ChartAreaHeight;
                }
                if ((x < ChartAreaLeft) || (x > ChartAreaLeft + ChartAreaWidth))
                {
                    continue;
                }

                DrawValueBar(
                    graphics,
                    s,
                    pair,
                    x - halfWidth,
                    ChartAreaBottom - barHeight,
                    barWidth,
                    barHeight,
                    seriesList[s].ForegroundColor,
                    true);
            }
        }

        if (ShowXAxisLabels)
        {
            for (var i = 1; i < 100000; i *= 10)
            {
                if (i < minX || i > maxX) continue;

                // Normalize the value to the range [0, 1]
                double normalizedValue = (Math.Log(i) - Math.Log(minX)) / (Math.Log(maxX) - Math.Log(minX));

                // Map the normalized value to the display width
                int x = (int)(normalizedValue * ChartAreaWidth) + ChartAreaLeft;

                graphics.DrawText(
                x + halfWidth,
                ChartAreaBottom + DefaultMargin + AxisStroke,
                alignmentH: HorizontalAlignment.Center,
                color: AxisLabelColor,
                text: i.ToString(),
                font: font);
            }
        }
    }

}
