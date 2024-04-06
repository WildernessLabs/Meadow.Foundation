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
    private bool _showXLabels = true;

    /// <summary>
    /// Creates a vertical bar chart instance
    /// </summary>
    /// <param name="left">The control's left position</param>
    /// <param name="top">The control's top position</param>
    /// <param name="width">The control's width</param>
    /// <param name="height">The control's height</param>
    public VerticalBarChart(int left, int top, int width, int height)
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
    public VerticalBarChart(int left, int top, int width, int height, float[] series)
        : base(left, top, width, height)
    {
        Series = series;
    }

    /// <summary>
    /// Gets or sets a series of float values to plot
    /// </summary>
    public float[]? Series
    {
        get => _series;
        set => SetInvalidatingProperty(ref _series, value);
    }

    /// <summary>
    /// Gets or sets the padding around a bar
    /// </summary>
    /// <remarks>This property behaves like a margin, so the space between any two bars will be twice this value</remarks>
    public int BarSpacing
    {
        get => _barSpacing;
        set => SetInvalidatingProperty(ref _barSpacing, value);
    }

    /// <summary>
    /// Gets or sets the string formatting (i.e. "N0" for a 0-decimal number) to apply to the X axis labels
    /// </summary>
    public string XAxisLabelFormat
    {
        get => _xlabelFormat;
        set => SetInvalidatingProperty(ref _xlabelFormat, value);
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
