using System.Collections.Generic;

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

        DrawXAxis(graphics);
        DrawYAxis(graphics);

        foreach (var series in Series)
        {
            DrawSeries(graphics, series);
        }

        graphics.Show();
    }

    private void DrawXAxis(MicroGraphics graphics)
    {
        // TODO: deal with chart with negative values

        // for now it's a fixed line at the bottom
        graphics.Stroke = DefaultAxisStroke;
        graphics.DrawLine(
            Left + DefaultMargin,
            Bottom - DefaultMargin - DefaultAxisStroke,
            Right - DefaultMargin,
            Bottom - DefaultMargin - DefaultAxisStroke,
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
        var chartHeight = Height - (2 * DefaultMargin);

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
            var scaledY = Bottom - (int)(((point.Y / yRange) * chartHeight) + DefaultMargin);

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
