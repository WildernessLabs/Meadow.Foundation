using System.Collections.Generic;

namespace Meadow.Foundation.Graphics.MicroLayout;

public class LineChartSeries
{
    public bool ShowLines { get; set; }
    public Color LineColor { get; set; }
    public int LineWidth { get; set; }

    public bool ShowPoints { get; set; }
    public Color PointColor { get; set; }
    public int PointSize { get; set; }

    public List<LineSeriesPoint> Points { get; set; } = new();
}
