namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A single point on a LineSeriesCHart
/// </summary>
public struct LineSeriesPoint
{
    // dev note: separate class from Point to allow setting point color/symbol in the future

    /// <summary>
    /// Creates a LineSeriesPoint
    /// </summary>
    public LineSeriesPoint()
    {
        X = Y = 0;
    }

    /// <summary>
    /// Creates a LineSeriesPoint
    /// </summary>
    /// <param name="x">The point's X value</param>
    /// <param name="y">The point's Y value</param>
    public LineSeriesPoint(double x, double y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Gets or sets the point's X value
    /// </summary>
    public double X { get; set; }
    /// <summary>
    /// Gets or sets the point's Y value
    /// </summary>
    public double Y { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"({X},{Y})";
    }
}
