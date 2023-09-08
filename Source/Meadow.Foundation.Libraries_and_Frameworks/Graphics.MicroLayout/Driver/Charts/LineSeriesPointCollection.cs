using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.Graphics.MicroLayout;

public class LineSeriesPointCollection : IEnumerable<LineSeriesPoint>
{
    private List<LineSeriesPoint> _points = new();

    public double MinX { get; private set; }
    public double MinY { get; private set; }
    public double MaxX { get; private set; }
    public double MaxY { get; private set; }

    public int Count => _points.Count;

    public void Add(double x, double y)
    {
        Add(new LineSeriesPoint(x, y));
    }

    public void Add(params LineSeriesPoint[] points)
    {
        lock (_points)
        {
            foreach (var point in points)
            {
                // do this now so we don't have to calculate during drawing
                if (point.X < MinX) MinX = point.X;
                if (point.X > MaxX) MaxX = point.X;
                if (point.Y < MinY) MinY = point.Y;
                if (point.Y > MaxY) MaxY = point.Y;

                _points.Add(point);
            }
        }
    }

    public IEnumerator<LineSeriesPoint> GetEnumerator()
    {
        return _points.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
