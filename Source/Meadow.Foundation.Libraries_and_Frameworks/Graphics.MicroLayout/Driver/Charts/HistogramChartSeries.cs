using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a series in a histogram chart.
/// </summary>
public class HistogramChartSeries : INotifyPropertyChanged
{
    private Color _foreColor = Color.White;
    private IEnumerable<(int X, int Y)> _elements = Array.Empty<(int X, int Y)>();

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the foreground color of the series.
    /// </summary>
    public Color ForeColor
    {
        get => _foreColor;
        set
        {
            if (value == ForeColor) { return; }
            _foreColor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForeColor)));
        }
    }

    /// <summary>
    /// Gets or sets the data elements of the series, where each element is a tuple containing X and Y values.
    /// </summary>
    public IEnumerable<(int X, int Y)> DataElements
    {
        get => _elements;
        set
        {
            if (value == DataElements) { return; }
            _elements = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataElements)));
        }
    }


}
