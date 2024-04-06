using System.Linq;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A vertical bar chart
/// </summary>
public class VerticalBarChart : ChartControl
{
    private float[]? _series;
    private int _barSpacing = 1;
    private string _xlabelFormat = "N1";
    private Color _seriesColor = Color.White;
    private bool _showXLabels;

    public VerticalBarChart(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
    }

    public VerticalBarChart(int left, int top, int width, int height, float[] series)
        : base(left, top, width, height)
    {
        Series = series;
    }

    public float[]? Series
    {
        get => _series;
        set => SetInvalidatingProperty(ref _series, value);
    }

    public int BarSpacing
    {
        get => _barSpacing;
        set => SetInvalidatingProperty(ref _barSpacing, value);
    }

    public string XAxisLabelFormat
    {
        get => _xlabelFormat;
        set => SetInvalidatingProperty(ref _xlabelFormat, value);
    }

    public bool ShowXAxisLabels
    {
        get => _showXLabels;
        set => SetInvalidatingProperty(ref _showXLabels, value);
    }

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

        ChartAreaTop = Top + DefaultMargin * 2 - AxisStroke;
        ChartAreaBottom = Bottom - DefaultMargin - AxisStroke;
        if (ShowXAxisLabels)
        {
            ChartAreaBottom -= font.Height;
        }
        ChartAreaHeight = Height - DefaultMargin * 3;

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

    private void DrawSeries(MicroGraphics graphics, float[] series, IFont font)
    {
        var barWidth = (ChartAreaWidth / series.Length) - 2 * BarSpacing;
        var halfWidth = barWidth / 2;

        var x = ChartAreaLeft + BarSpacing;

        var heightScale = ChartAreaHeight * 0.9f / series.Max();

        foreach (var item in series)
        {
            var barHeight = (int)(heightScale * item);

            graphics.DrawRectangle(
                x,
                ChartAreaBottom - barHeight,
                barWidth,
                barHeight,
                color: SeriesColor,
                filled: true);

            if (ShowXAxisLabels)
            {
                graphics.DrawText(
                x + halfWidth,
                ChartAreaBottom + DefaultMargin + AxisStroke,
                alignmentH: HorizontalAlignment.Center,
                color: AxisLabelColor,
                text: item.ToString(XAxisLabelFormat),
                font: font);
            }

            x += barWidth + 2 * BarSpacing;
        }
    }
}
