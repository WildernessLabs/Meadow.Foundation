namespace Meadow.Foundation.Graphics.MicroLayout;

public struct LineSeriesPoint
{
    // dev note: separate class from Point to allow setting point color/symbol in the future

    public LineSeriesPoint()
    {
        X = Y = 0;
    }

    public LineSeriesPoint(double x, double y)
    {
        X = x;
        Y = y;
    }

    public double X { get; set; }
    public double Y { get; set; }

    public override string ToString()
    {
        return $"({X},{Y})";
    }
}
