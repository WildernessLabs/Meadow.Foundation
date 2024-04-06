namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// An abstract base class for Chart controls
/// </summary>
public abstract class ChartControl : ThemedControl
{
    private IFont _axisFont = default!;

    /// <summary>
    /// The default color for axis lines
    /// </summary>
    public static Color DefaultAxisColor = Color.Gray;

    /// <summary>
    /// The default color for axis labels
    /// </summary>
    public static Color DefaultAxisLabelColor = Color.White;

    /// <summary>
    /// The default chart background color
    /// </summary>
    public static Color DefaultBackgroundColor = Color.Black;

    /// <summary>
    /// The default width of the control margin
    /// </summary>
    protected const int DefaultMargin = 5;

    /// <summary>
    /// The default width of the control's axis lines
    /// </summary>
    protected const int DefaultAxisStroke = 4;

    /// <summary>
    /// The IFont used to for displaying axis labels
    /// </summary>
    public IFont? AxisFont { get; set; }
    /// <summary>
    /// The chart's background color
    /// </summary>
    public Color BackgroundColor { get; set; } = DefaultBackgroundColor;
    /// <summary>
    /// The color used to draw the chart axes lines
    /// </summary>
    public Color AxisColor { get; set; } = DefaultAxisColor;
    /// <summary>
    /// The color used to draw the chart axes labels
    /// </summary>
    public Color AxisLabelColor { get; set; } = DefaultAxisLabelColor;
    /// <summary>
    /// The width of the axes lines
    /// </summary>
    public int AxisStroke { get; set; } = DefaultAxisStroke;

    protected int ChartAreaHeight { get; set; }
    protected int ChartAreaWidth { get; set; }
    protected int ChartAreaLeft { get; set; }
    protected int ChartAreaTop { get; set; }
    protected int ChartAreaBottom { get; set; }
    protected int ParentOffsetX => (Parent?.Left ?? 0);
    protected int ParentOffsetY => (Parent?.Top ?? 0);

    /// <summary>
    /// Creates a DisplayLineChart instance
    /// </summary>
    /// <param name="left">The control's left position</param>
    /// <param name="top">The control's top position</param>
    /// <param name="width">The control's width</param>
    /// <param name="height">The control's height</param>
    public ChartControl(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
    }

    protected IFont GetAxisFont()
    {
        if (AxisFont == null)
        {
            _axisFont = new Font6x8();
        }
        else
        {
            _axisFont = AxisFont;
        }

        return _axisFont;
    }

    /// <inheritdoc/>
    public override void ApplyTheme(DisplayTheme theme)
    {
    }
}
