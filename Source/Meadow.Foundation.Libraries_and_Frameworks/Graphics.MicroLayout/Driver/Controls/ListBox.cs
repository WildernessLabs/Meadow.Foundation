using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a list box control in the user interface.
/// </summary>
public class ListBox : MicroLayout
{
    private int _selectedIndex = -1;
    private int _topIndex = 0;
    private readonly List<string> _items = new();
    private Color _textColor = Color.White;
    private Color _selectedRowColor = Color.LightGray;
    private Color _selectedTextColor = Color.Black;
    private readonly int _rowHeight;
    private readonly IFont _font;
    private int _selectedLabelIndex = -1;

    /// <summary>
    /// Spacing, in pixels, between items
    /// </summary>
    public int ItemSpacing { get; } = 1;
    /// <summary>
    /// Items to display in the ListBox
    /// </summary>
    public ObservableCollection<string> Items { get; } = new();

    /// <summary>
    /// Creates a ListBox control
    /// </summary>
    /// <param name="left"></param>
    /// <param name="top"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="font"></param>
    public ListBox(int left, int top, int width, int height, IFont font)
        : base(left, top, width, height)
    {
        _font = font;
        BackgroundColor = Color.Black;
        _rowHeight = font.Height + ItemSpacing;
        var rowCount = this.Height / _rowHeight;
        CreateRowlabels(rowCount);
        Items.CollectionChanged += OnItemsCollectionChanged;
    }

    private void CreateRowlabels(int rowCount)
    {
        var y = 0;
        for (var i = 0; i < rowCount; i++)
        {
            Controls.Add(
                new Label(Left, Top + y, this.Width, _rowHeight)
                {
                    Font = _font,
                    TextColor = TextColor,
                    BackColor = this.BackgroundColor ?? Color.Transparent
                });

            y += _rowHeight;
        }
    }

    /// <summary>
    /// Gets or sets the background color for a selected row
    /// </summary>
    public Color SelectedRowColor
    {
        get => _selectedRowColor;
        set
        {
            _selectedRowColor = value;

            if (_selectedIndex >= 0)
            {
                (Controls[_selectedIndex] as Label)!.BackColor = SelectedRowColor;
            }
        }
    }

    /// <summary>
    /// Gets or sets the background color for a selected row
    /// </summary>
    public Color SelectedTextColor
    {
        get => _selectedTextColor;
        set
        {
            _selectedTextColor = value;

            if (_selectedIndex >= 0)
            {
                (Controls[_selectedIndex] as Label)!.TextColor = SelectedTextColor;
            }
        }
    }

    /// <summary>
    /// Gets or sets the foreground color of list items
    /// </summary>
    public Color TextColor
    {
        get => _textColor;
        set
        {
            _textColor = value;

            foreach (Label label in Controls)
            {
                label.TextColor = TextColor;
            }
        }
    }

    /// <summary>
    /// The value of the selected Item
    /// </summary>
    public string? SelectedItem
    {
        get
        {
            if (SelectedIndex < 0) return null;
            return Items[SelectedIndex];
        }
    }

    /// <summary>
    /// The index of the currently selected Item
    /// </summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            SetInvalidatingProperty(ref _selectedIndex, value);
            var controlIndex = SelectedIndex - _topIndex;
            if (controlIndex >= 0 && controlIndex < Items.Count)
            {
                SelectLabel(controlIndex);
            }
            else
            {
                SelectLabel(-1);
            }
        }
    }

    private void SelectLabel(int index)
    {
        if (_selectedLabelIndex >= 0)
        {
            (Controls[_selectedLabelIndex] as Label)!.TextColor = TextColor;
            (Controls[_selectedLabelIndex] as Label)!.BackColor = BackgroundColor ?? Color.Transparent;
        }
        if (index >= 0)
        {
            (Controls[index] as Label)!.TextColor = SelectedTextColor;
            (Controls[index] as Label)!.BackColor = SelectedRowColor;
        }
        _selectedLabelIndex = index;
    }

    /// <summary>
    /// Gets or sets the index of the top visible Item
    /// </summary>
    public int TopIndex
    {
        get => _topIndex;
        set => SetInvalidatingProperty(ref _topIndex, value);
    }

    private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                // is the added item visible?
                if (e.NewStartingIndex < TopIndex + Controls.Count)
                {
                    var i = e.NewStartingIndex - TopIndex;
                    foreach (var item in e.NewItems)
                    {
                        (Controls[i] as Label)!.Text = item.ToString();
                    }
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldStartingIndex < TopIndex + Controls.Count)
                {
                    var i = e.OldStartingIndex - TopIndex;

                    var mustClearSelection = _selectedLabelIndex == i;
                    var mustMoveSelection = (_selectedLabelIndex > i);

                    while (i < Items.Count && i < Controls.Count - 1)
                    {
                        (Controls[i] as Label)!.Text = (Controls[i + 1] as Label)!.Text;
                        i++;
                    }
                    (Controls[i] as Label)!.Text = string.Empty;

                    if (mustClearSelection)
                    {
                        SelectLabel(-1);
                    }
                    else if (mustMoveSelection)
                    {
                        SelectLabel(_selectedLabelIndex - 1);
                    }
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                foreach (Label label in Controls)
                {
                    label.Text = string.Empty;
                }
                break;
        }
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
