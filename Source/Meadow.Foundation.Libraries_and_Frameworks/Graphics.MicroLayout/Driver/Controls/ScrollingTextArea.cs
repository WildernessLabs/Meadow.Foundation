using System.Linq;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents an auto-scrolling text area
/// </summary>
public class ScrollingTextArea : LayoutBase
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
    /// The number of text rows that fit within the visible height of the control, based on the font height and item spacing.
    /// </summary>
    public int RowCount { get; }

    /// <summary>
    /// The text color applied to rows that have not been given an explicit color override.
    /// </summary>
    public Color DefaultRowColor { get; set; }

    /// <summary>
    /// Creates a new ScrollingTextArea
    /// </summary>
    public ScrollingTextArea(int left, int top, int width, int height, IFont font)
        : base(left, top, width, height)
    {
        _font = font;
        _rowHeight = font.Height + ItemSpacing;

        BackgroundColor = Color.Black;
        DefaultRowColor = Color.LightGray;
        RowCount = Height / _rowHeight;

        _labels = new Label[RowCount];

        CreateRowLabels(RowCount);
    }

    /// <inheritdoc/>
    public override IControl? Parent
    {
        get => base.Parent;
        set => base.Parent = value;
    }

    private void CreateRowLabels(int rowCount)
    {
        var currentRelativeY = 0;

        for (var i = 0; i < rowCount; i++)
        {
            _labels[i] = new Label(
                0,
                currentRelativeY,
                Width,
                _rowHeight)
            {
                Font = _font,
                TextColor = DefaultRowColor,
                BackgroundColor = BackgroundColor ?? Color.Transparent,
                VerticalAlignment = VerticalAlignment.Center,
                Text = string.Empty
            };

            _labels[i].Parent = this;

            Controls.Add(_labels[i]);

            currentRelativeY += _rowHeight;
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
        foreach (Label label in Controls.Cast<Label>())
        {
            label.ApplyTheme(theme);
        }
    }

    /// <inheritdoc/>
    internal override void PerformLayout()
    {
        var currentRelativeY = 0;

        foreach (var label in _labels)
        {
            if (label == null)
            {
                continue;
            }

            label.Left = 0;
            label.Top = currentRelativeY;
            label.Width = Width;
            label.Height = _rowHeight;
            currentRelativeY += _rowHeight;
        }
    }

    protected override void OnDraw(MicroGraphics graphics)
    {
        base.OnDraw(graphics);
    }
}