using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A Collection of LineSeriesPoints
/// </summary>
public class LineSeriesPointCollection : IEnumerable<LineSeriesPoint>
{
    private readonly List<LineSeriesPoint> _points = new();

    /// <summary>
    /// Gets the minimum X value in the collection
    /// </summary>
    public double MinX { get; private set; }
    /// <summary>
    /// Gets the minimum Y value in the collection
    /// </summary>
    public double MinY { get; private set; }
    /// <summary>
    /// Gets the maximum X value in the collection
    /// </summary>
    public double MaxX { get; private set; }
    /// <summary>
    /// Gets the maximum Y value in the collection
    /// </summary>
    public double MaxY { get; private set; }

    /// <summary>
    /// Gets the number of points in the collection
    /// </summary>
    public int Count => _points.Count;

    /// <summary>
    /// Adds a point to the collection
    /// </summary>
    /// <param name="x">The point's X value</param>
    /// <param name="y">The point's Y value</param>
    public void Add(double x, double y)
    {
        Add(new LineSeriesPoint(x, y));
    }

    /// <summary>
    /// Adds a point to the collection
    /// </summary>
    /// <param name="points">The point to add</param>
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

    /// <summary>
    /// Removes a point to the collection
    /// </summary>
    /// <param name="point">The point to remove</param>
    public void Remove(LineSeriesPoint point)
    {
        Remove(point);
    }

    /// <summary>
    /// Removes a point to the collection
    /// </summary>
    /// <param name="points">The points to remove</param>
    public void Remove(params LineSeriesPoint[] points)
    {
        lock (_points)
        {
            foreach (var point in points)
            {
                _points.Remove(point);
            }
        }
    }

    /// <summary>
    /// Removes all points to the collection
    /// </summary>
    public void Clear()
    {
        lock (_points)
        {
            _points.Clear();
        }
    }

    /// <inheritdoc/>
    public IEnumerator<LineSeriesPoint> GetEnumerator()
    {
        return _points.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
