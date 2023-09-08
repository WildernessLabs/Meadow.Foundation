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
