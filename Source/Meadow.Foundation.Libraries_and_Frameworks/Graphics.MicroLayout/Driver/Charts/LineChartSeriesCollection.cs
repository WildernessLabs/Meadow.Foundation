using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A collection of LineCHartSeries instances
/// </summary>
public class LineChartSeriesCollection : IEnumerable<LineChartSeries>
{
    private readonly List<LineChartSeries> _series = new();

    /// <summary>
    /// Adds one or more LineChartSeries to the collection
    /// </summary>
    /// <param name="series"></param>
    public void Add(params LineChartSeries[] series)
    {
        _series.AddRange(series);
    }

    /// <summary>
    /// Removes a LineChartSeries from the collection
    /// </summary>
    /// <param name="series"></param>
    public void Remove(LineChartSeries series)
    {
        _series.Remove(series);
    }

    /// <summary>
    /// Removes all LineChartSeries from the collection
    /// </summary>
    public void Clear()
    {
        _series.Clear();
    }

    /// <inheritdoc/>
    public LineChartSeries this[int index]
    {
        get => _series[index];
    }

    /// <inheritdoc/>
    public IEnumerator<LineChartSeries> GetEnumerator()
    {
        return _series.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}