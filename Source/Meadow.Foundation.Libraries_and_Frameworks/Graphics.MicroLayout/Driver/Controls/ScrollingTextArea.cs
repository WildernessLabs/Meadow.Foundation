namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents an auto-scrolling text area
/// </summary>
public class ScrollingTextArea : MicroLayout
{
    private readonly int _rowHeight;
    private readonly IFont _font;
    private readonly Label[] _labels;
    private int _currentRow = 0;

    /// <summary>
    /// Spacing, in pixels, between items
    /// </summary>
    public int ItemSpacing { get; } = 1;

    /// <summary>
    /// Gets the number of rows shown
    /// </summary>
    public int RowCount { get; }

    /// <summary>
    /// Gets or sets the default row text color
    /// </summary>
    public Color DefaultRowColor { get; set; }

    public ScrollingTextArea(int left, int top, int width, int height, IFont font)
        : base(left, top, width, height)
    {
        _font = font;
        _rowHeight = font.Height + ItemSpacing;

        BackgroundColor = Color.Black;
        DefaultRowColor = Color.LightGray;
        RowCount = this.Height / _rowHeight;

        _labels = new Label[RowCount];

        CreateRowLabels(RowCount);
    }

    private void CreateRowLabels(int rowCount)
    {
        var y = 0;
        for (var i = 0; i < rowCount; i++)
        {
            _labels[i] =
                new Label(Left, Top + y, this.Width, _rowHeight)
                {
                    Font = _font,
                    TextColor = DefaultRowColor,
                    BackColor = this.BackgroundColor ?? Color.Transparent,
                    VerticalAlignment = VerticalAlignment.Center,
                };

            Controls.Add(_labels[i]);

            y += _rowHeight;
        }
    }

    /// <summary>
    /// Clears all rows in the control
    /// </summary>
    public void Clear()
    {
        foreach (var label in _labels)
        {
            label.Text = string.Empty;
        }
        _currentRow = 0;
    }

    /// <summary>
    /// Adds a tew test item to the end/bottom of the control
    /// </summary>
    /// <param name="message">The message to add</param>
    /// <param name="color">The (optional) color for the row</param>
    public void Add(string message, Color? color = null)
    {
        //_screen.BeginUpdate();

        while (_currentRow >= RowCount)
        {
            for (var r = 0; r < RowCount - 1; r++)
            {
                _labels[r].Text = _labels[r + 1].Text;
                _labels[r].TextColor = _labels[r + 1].TextColor;
            }
            _currentRow--;
        }

        _labels[_currentRow].Text = message;
        _labels[_currentRow].TextColor = color ?? DefaultRowColor;

        _currentRow++;
    }

    /// <inheritdoc/>
    public override void ApplyTheme(DisplayTheme theme)
    {
        foreach (Label label in Controls)
        {
            label.ApplyTheme(theme);
        }
    }

    /// <inheritdoc/>
    protected override void OnDraw(MicroGraphics graphics)
    {
    }
}
