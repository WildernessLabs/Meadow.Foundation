using System.Linq;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// An X/Y Line chart
/// </summary>
public class LineChart : ChartControl
{
    /// <summary>
    /// When true, Y-value origin (zero) is always displayed, otherwise the Y axis is scaled based on the data range.
    /// </summary>
    public bool AlwaysShowYOrigin { get; set; } = true;

    //    public bool ShowXAxisLabels { get; set; }
    /// <summary>
    /// When true, Y-axis labels will be shown
    /// </summary>
    public bool ShowYAxisLabels { get; set; }
    /// <summary>
    /// The collection of data series to plot
    /// </summary>
    public LineChartSeriesCollection Series { get; set; } = new();

    private double VerticalScale { get; set; } // pixels per Y units
    private double YMinimumValue { get; set; } // Y units at bottom of chart
    private double YMaximumValue { get; set; } // Y units at top of chart
    private double XAxisYIntercept { get; set; }
    private int XAxisScaledPosition { get; set; }
    private double HorizontalScale { get; set; } // pixels per X units
    private double HorizontalMinimum { get; set; } // X units at left of chart

    /// <summary>
    /// Creates a DisplayLineChart instance
    /// </summary>
    /// <param name="left">The control's left position</param>
    /// <param name="top">The control's top position</param>
    /// <param name="width">The control's width</param>
    /// <param name="height">The control's height</param>
    public LineChart(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
    }

    /// <inheritdoc/>
    protected override void OnDraw(MicroGraphics graphics)
    {
        graphics.DrawRectangle(Left, Top, Width, Height, BackgroundColor, true);

        ChartAreaTop = Top + DefaultMargin * 2 - AxisStroke;
        ChartAreaBottom = Bottom - DefaultMargin;

        if (Series.Count > 0)
        {
            // determine overall min/max
            var minX = Series.Min(s => s.Points.MinX);
            var maxX = Series.Max(s => s.Points.MaxX);

            if (AlwaysShowYOrigin)
            {
                var min = Series.Min(s => s.Points.MinY);
                var max = Series.Max(s => s.Points.MaxY);

                if (min > 0)
                {
                    YMinimumValue = 0;
                    YMaximumValue = max;
                }
                else if (max < 0)
                {
                    YMinimumValue = min;
                    YMaximumValue = 0;
                }
                else
                {
                    YMinimumValue = min;
                    YMaximumValue = max;
                }
            }
            else
            {
                // set chart top/bottom at 10% above/below the min/max
                var ymin = Series.Min(s => s.Points.MinY);
                YMinimumValue = (ymin > 0) ? ymin * 0.9 : ymin * 1.1;
                var ymax = Series.Max(s => s.Points.MaxY);
                YMaximumValue = (ymax > 0) ? ymax * 1.1 : ymax * 0.9;
            }

            ChartAreaHeight = Height - DefaultMargin * 3;
            VerticalScale = ChartAreaHeight / (YMaximumValue - YMinimumValue); // pixels per vertical unit

            DrawYAxis(graphics);
            DrawXAxis(graphics, YMinimumValue, YMaximumValue);

            foreach (var series in Series)
            {
                DrawSeries(graphics, series);
            }

            DrawAxisLabels(graphics);
        }

        graphics.Show();
    }

    private void DrawAxisLabels(MicroGraphics graphics)
    {
        var font = GetAxisFont();

        if (ShowYAxisLabels)
        {
            // axis label
            if (XAxisYIntercept != YMinimumValue)
            {
                graphics.DrawText(
                    x: Left + DefaultMargin + ParentOffsetX,
                    y: XAxisScaledPosition - (font.Height / 2) + +ParentOffsetY, // centered on tick
                    color: AxisLabelColor,
                     text: XAxisYIntercept.ToString("0.0"),
                    font: font);
            }

            // max label
            graphics.DrawText(
                x: Left + DefaultMargin + ParentOffsetX,
                y: ChartAreaTop - font.Height + DefaultMargin,
                color: AxisLabelColor,
                text: YMaximumValue.ToString("0.0"),
                font: font);

            // min label
            graphics.DrawText(
                x: Left + DefaultMargin + ParentOffsetX,
                y: ChartAreaBottom - font.Height + ParentOffsetY,
                color: AxisLabelColor,
                text: YMinimumValue.ToString("0.0"),
                font: font);
        }
    }

    private void DrawXAxis(MicroGraphics graphics, double minY, double maxY)
    {
        if (minY < 0 && maxY > 0)
        {
            // axis is at 0
            XAxisYIntercept = 0;

            XAxisScaledPosition = Bottom - DefaultMargin + (int)(minY * VerticalScale);
        }
        else
        {
            // axis at min Y
            XAxisYIntercept = YMinimumValue;

            XAxisScaledPosition = ChartAreaBottom - DefaultMargin * 2 + AxisStroke * 2;
        }

        // for now it's a fixed line at the bottom
        graphics.Stroke = AxisStroke;
        graphics.DrawHorizontalLine(
            ChartAreaLeft, //ChartAreaLeft - AxisStroke + ParentOffsetX,
            XAxisScaledPosition,
            ChartAreaWidth,
            AxisColor);
    }

    private void DrawYAxis(MicroGraphics graphics)
    {
        var leftMargin = DefaultMargin + AxisStroke;

        if (ShowYAxisLabels)
        {
            // TODO: this needs to be label-based
            leftMargin += GetAxisFont().Width * YMaximumValue.ToString("0.0").Length;
        }

        // TODO: deal with chart with negative values
        ChartAreaLeft = Left + leftMargin + AxisStroke;
        ChartAreaWidth = Right - ChartAreaLeft - DefaultMargin - AxisStroke;

        // for now it's a fixed line at the left
        graphics.Stroke = AxisStroke;
        graphics.DrawVerticalLine(
            ChartAreaLeft - AxisStroke,
            ChartAreaTop,
            ChartAreaHeight + AxisStroke,
            AxisColor);
    }

    private void DrawSeries(MicroGraphics graphics, LineChartSeries series)
    {
        var minX = series.Points.MinX;
        var minY = series.Points.MinY;
        var xRange = series.Points.MaxX - minX;
        var yRange = series.Points.MaxY; //  - minY; // assuming axis at 0 right now

        int lastX = 0;
        int lastY = 0;
        var first = true;

        graphics.Stroke = series.LineStroke;

        foreach (var point in series.Points)
        {
            var scaledX = ChartAreaLeft + (int)(point.X / xRange * ChartAreaWidth);
            var scaledY = (ChartAreaTop + ChartAreaHeight) - (int)((point.Y - YMinimumValue) * VerticalScale);

            if (series.ShowLines)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    graphics.DrawLine(
                        lastX + ParentOffsetX,
                        lastY + ParentOffsetY,
                        scaledX,
                        scaledY,
                        series.LineColor);
                }

                lastX = scaledX;
                lastY = scaledY;
            }

            if (series.ShowPoints)
            {
                graphics.DrawCircle(
                    scaledX + ParentOffsetX,
                    scaledY + ParentOffsetY,
                    series.PointSize,
                    series.PointColor,
                    true);
            }
        }
    }
}