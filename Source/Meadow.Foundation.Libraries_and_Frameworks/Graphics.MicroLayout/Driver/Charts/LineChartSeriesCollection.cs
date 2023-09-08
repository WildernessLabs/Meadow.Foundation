using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.Graphics.MicroLayout;

public class LineChartSeriesCollection : IEnumerable<LineChartSeries>
{
    private readonly List<LineChartSeries> _series = new();

    public void Add(params LineChartSeries[] series)
    {
        _series.AddRange(series);
    }

    public IEnumerator<LineChartSeries> GetEnumerator()
    {
        return _series.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
