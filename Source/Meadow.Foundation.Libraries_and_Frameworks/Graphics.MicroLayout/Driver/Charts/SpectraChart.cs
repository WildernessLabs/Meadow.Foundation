using System;
using System.Linq;

namespace Meadow.Foundation.Graphics.MicroLayout;

public class SpectraChart : ChartControl
{
    private (int x, int y)[]? _series;
    private Color _seriesColor = Color.White;
    private bool _showXLabels = true;

    /// <summary>
    /// Creates a vertical bar chart instance
    /// </summary>
    /// <param name="left">The control's left position</param>
    /// <param name="top">The control's top position</param>
    /// <param name="width">The control's width</param>
    /// <param name="height">The control's height</param>
    public SpectraChart(int left, int top, int width, int height)
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
    public SpectraChart(int left, int top, int width, int height, (int x, int y)[] series)
        : base(left, top, width, height)
    {
        Series = series;
    }

    /// <summary>
    /// Gets or sets a series of float values to plot
    /// </summary>
    public (int x, int y)[]? Series
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
    /// Gets or sets the color of the plotted series (bars)
    /// </summary>
    public Color SeriesColor
    {
        get => _seriesColor;
        set => SetInvalidatingProperty(ref _seriesColor, value);
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

        int yLabelWidth = 0;
        if (Series != null)
        {
            // if we add Y labels (future feature), turn this on
            //yLabelWidth = graphics.MeasureText(Series.Max().ToString(XAxisLabelFormat), font).Width;
        }
        ChartAreaLeft = Left + DefaultMargin + yLabelWidth;

        ChartAreaWidth = Width - DefaultMargin;

        if (Series != null)
        {
            DrawSeries(graphics, Series, font);
        }

        graphics.DrawRectangle(
            ChartAreaLeft,
            ChartAreaBottom,
            ChartAreaWidth - DefaultMargin,
            AxisStroke,
            color: AxisColor,
            filled: true);

    }

    private static int CalculateLogarithmicPosition(double value, double minValue, double maxValue, int displayWidth)
    {
        if (value <= 0)
        {
            throw new ArgumentException("Value must be greater than 0 to calculate a logarithm.");
        }

        // Normalize the value to the range [0, 1]
        double normalizedValue = (Math.Log(value) - Math.Log(minValue)) / (Math.Log(maxValue) - Math.Log(minValue));

        // Map the normalized value to the display width
        int xPosition = (int)(normalizedValue * displayWidth);

        return xPosition;
    }

    private void DrawSeries(MicroGraphics graphics, (int X, int Y)[] series, IFont font)
    {
        var barWidth = 3;
        var halfWidth = barWidth / 2;

        var minX = series.Min(s => s.X);
        var maxX = series.Max(s => s.X);
        var maxY = series.Max(s => s.Y);

        var heightScale = ChartAreaHeight * 0.9f / maxY;

        foreach (var pair in series)
        {
            // Normalize the value to the range [0, 1]
            double normalizedValue = (Math.Log(pair.X) - Math.Log(minX)) / (Math.Log(maxX) - Math.Log(minX));

            // Map the normalized value to the display width
            int x = (int)(normalizedValue * ChartAreaWidth) + ChartAreaLeft;

            var barHeight = (int)(heightScale * pair.Y);

            graphics.DrawRectangle(
                x,
                ChartAreaBottom - barHeight,
                barWidth,
                barHeight,
                color: SeriesColor,
                filled: true);
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
