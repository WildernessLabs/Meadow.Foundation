using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.Graphics.MicroLayout;

public class DisplayLineChart : DisplayControl
{
    private const int DefaultMargin = 5;
    private const int DefaultAxisStroke = 5;

    public static Color DefaultAxisColor = Color.WhiteSmoke;
    public static Color DefaultBackgroundColor = Color.Black;

    public Color BackgroundColor { get; set; } = DefaultBackgroundColor;
    public Color AxisColor { get; set; } = DefaultAxisColor;
    public List<LineChartSeries> Series { get; set; } = new();

    public DisplayLineChart(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
    }

    public override void ApplyTheme(DisplayTheme theme)
    {
    }

    protected override void OnDraw(MicroGraphics graphics)
    {
        graphics.Clear(BackgroundColor);

        // determine overall min/max
        var minX = Series.Min(s => s.MinX());
        var maxX = Series.Max(s => s.MaxX());


        if (AlwaysShowYOrigin)
        {
            var min = Series.Min(s => s.MinY());
            var max = Series.Max(s => s.MaxY());

            if (min > 0)
            {
                VerticalMinimum = 0;
                VerticalMaximum = max;
            }
            else if (max < 0)
            {
                VerticalMinimum = min;
                VerticalMaximum = 0;
            }
            else
            {
                VerticalMinimum = min;
                VerticalMaximum = max;
            }
        }
        else
        {
            VerticalMinimum = Series.Min(s => s.MinY());
            VerticalMaximum = Series.Max(s => s.MaxY());
        }

        ChartAreaHeight = this.Height - (2 * DefaultMargin) - (DefaultAxisStroke / 2);
        VerticalScale = ChartAreaHeight / (VerticalMaximum - VerticalMinimum); // pixels per vertical unit

        DrawXAxis(graphics, VerticalMinimum, VerticalMaximum);
        DrawYAxis(graphics);

        foreach (var series in Series)
        {
            DrawSeries(graphics, series);
        }

        graphics.Show();
    }

    public bool AlwaysShowYOrigin { get; set; } = true;
    private double VerticalScale { get; set; } // pixels per Y units
    private double VerticalMinimum { get; set; } // Y units at bottom of chart
    private double VerticalMaximum { get; set; } // Y units at top of chart
    private double HorizontalScale { get; set; } // pixels per X units
    private double HorizontalMinimum { get; set; } // X units at left of chart
    private int ChartAreaHeight { get; set; }

    private void DrawXAxis(MicroGraphics graphics, double minY, double maxY)
    {
        int scaledYOffset;

        if (minY < 0 && maxY > 0)
        {
            // axis is at 0

            scaledYOffset = Bottom - DefaultMargin - DefaultAxisStroke + (int)(minY * VerticalScale);
        }
        else
        {

            // axis at min Y
            scaledYOffset = Bottom - DefaultMargin - DefaultAxisStroke;
        }

        // for now it's a fixed line at the bottom
        graphics.Stroke = DefaultAxisStroke;
        graphics.DrawLine(
            Left + DefaultMargin,
            scaledYOffset,
            Right - DefaultMargin,
            scaledYOffset,
            AxisColor);
    }

    private void DrawYAxis(MicroGraphics graphics)
    {
        // TODO: deal with chart with negative values

        // for now it's a fixed line at the left
        graphics.Stroke = DefaultAxisStroke;
        graphics.DrawLine(
            Left + DefaultMargin,
            Top + DefaultMargin,
            Left + DefaultMargin,
            Bottom - DefaultMargin,
            AxisColor);
    }

    private void DrawSeries(MicroGraphics graphics, LineChartSeries series)
    {
        var chartWidth = Width - (2 * DefaultMargin);

        var minX = series.MinX();
        var minY = series.MinY();
        var xRange = series.MaxX() - minX;
        var yRange = series.MaxY(); //  - minY; // assuming axis at 0 right now

        LineSeriesPoint lastPoint = new LineSeriesPoint();
        var first = true;

        graphics.Stroke = series.LineWidth;

        foreach (var point in series.Points)
        {
            var scaledX = (int)((point.X / xRange) * chartWidth) + DefaultMargin;
            var scaledY = Bottom - DefaultMargin - DefaultAxisStroke / 2 - (int)((point.Y - VerticalMinimum) * VerticalScale);

            if (series.ShowLines)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    graphics.DrawLine(
                        (int)lastPoint.X,
                        (int)lastPoint.Y,
                        scaledX,
                        scaledY,
                        series.LineColor);
                }

                lastPoint.X = scaledX;
                lastPoint.Y = scaledY;
            }

            if (series.ShowPoints)
            {
                graphics.DrawCircle(scaledX, scaledY, series.PointSize, series.PointColor, true);
            }
        }
    }
}
