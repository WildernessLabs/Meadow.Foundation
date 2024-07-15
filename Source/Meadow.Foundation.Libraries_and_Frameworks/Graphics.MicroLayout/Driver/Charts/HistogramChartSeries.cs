using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Meadow.Foundation.Graphics.MicroLayout;

public class HistogramChartSeries : INotifyPropertyChanged
{
    private Color _foreColor = Color.White;
    private IEnumerable<(int X, int Y)> _elements = Array.Empty<(int X, int Y)>();

    public event PropertyChangedEventHandler PropertyChanged;

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
