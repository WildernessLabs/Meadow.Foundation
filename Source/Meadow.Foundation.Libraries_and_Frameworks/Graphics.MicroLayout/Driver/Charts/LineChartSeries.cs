namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A series for a DisplayLineChart
/// </summary>
public class LineChartSeries
{
    /// <summary>
    /// Gets or sets whether lines are displayed between points
    /// </summary>
    public bool ShowLines { get; set; }
    /// <summary>
    /// Gets or sets the color of lines between points
    /// </summary>
    public Color LineColor { get; set; }
    /// <summary>
    /// Gets or sets the width of lines between points
    /// </summary>
    public int LineStroke { get; set; }

    /// <summary>
    /// Gets or sets whether markers are displayed for points
    /// </summary>
    public bool ShowPoints { get; set; }
    /// <summary>
    /// Gets or sets the color of markers displayed for points
    /// </summary>
    public Color PointColor { get; set; }
    /// <summary>
    /// Gets or sets the size of markers displayed for points
    /// </summary>
    public int PointSize { get; set; }

    /// <summary>
    /// Gets or sets the points in the series
    /// </summary>
    public LineSeriesPointCollection Points { get; set; } = new();
}
