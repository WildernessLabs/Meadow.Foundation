using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A layout that arranges child controls in a grid with alignment and spanning options.
/// </summary>
public class GridLayout : MicroLayout
{
    /// <summary>
    /// Defines the alignment options for controls within grid cells.
    /// </summary>
    public enum Alignment
    {
        /// <summary>
        /// Align left
        /// </summary>
        Left,
        /// <summary>
        /// Align to top
        /// </summary>
        Top,
        /// <summary>
        /// align right
        /// </summary>
        Right,
        /// <summary>
        /// Align bottom
        /// </summary>
        Bottom,
        /// <summary>
        /// Center
        /// </summary>
        Center,
        /// <summary>
        /// Stretch to fill
        /// </summary>
        Stretch
    }

    private readonly int _rows;
    private readonly int _columns;
    private readonly Dictionary<IControl, (int row, int col, int rowspan, int colspan, Alignment alignment)> _controlPositions = new();

    /// <summary>
    /// Gets or sets the spacing between rows.
    /// </summary>
    public int RowSpacing
    {
        get => _rowSpacing;
        set
        {
            _rowSpacing = value;
            LayoutControls();
            Invalidate();
        }
    }
    int _rowSpacing = 2;

    /// <summary>
    /// Gets or sets the spacing between columns.
    /// </summary>
    public int ColumnSpacing
    {
        get => _colSpacing;
        set
        {
            _colSpacing = value;
            LayoutControls();
            Invalidate();
        }
    }
    int _colSpacing = 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="GridLayout"/> class.
    /// </summary>
    /// <param name="left">The left position of the grid.</param>
    /// <param name="top">The top position of the grid.</param>
    /// <param name="width">The width of the grid.</param>
    /// <param name="height">The height of the grid.</param>
    /// <param name="rows">The number of rows in the grid.</param>
    /// <param name="columns">The number of columns in the grid.</param>
    public GridLayout(int left, int top, int width, int height, int rows, int columns)
        : base(left, top, width, height)
    {
        if (rows <= 0 || columns <= 0)
        {
            throw new ArgumentException("Rows and columns must be greater than zero.");
        }

        _rows = rows;
        _columns = columns;
    }

    /// <summary>
    /// Adds a control to the grid at the specified position with optional spanning and alignment.
    /// </summary>
    /// <param name="control">The control to add.</param>
    /// <param name="row">The row index of the control.</param>
    /// <param name="col">The column index of the control.</param>
    /// <param name="rowspan">The number of rows the control spans.</param>
    /// <param name="colspan">The number of columns the control spans.</param>
    /// <param name="alignment">The alignment of the control within the cell.</param>
    public void Add(IControl control, int row, int col, int rowspan = 1, int colspan = 1, Alignment alignment = Alignment.Center)
    {
        if (row < 0 || row >= _rows || col < 0 || col >= _columns || row + rowspan > _rows || col + colspan > _columns)
        {
            throw new ArgumentOutOfRangeException("Row, column, rowspan, or colspan is out of range.");
        }

        Controls.Add(control);
        _controlPositions[control] = (row, col, rowspan, colspan, alignment);
        SetControlPosition(control, row, col, rowspan, colspan, alignment);
    }

    /// <summary>
    /// Removes a control from the grid layout.
    /// </summary>
    /// <param name="control">The control to remove.</param>
    public void Remove(IControl control)
    {
        if (_controlPositions.ContainsKey(control))
        {
            _controlPositions.Remove(control);
            Controls.Remove(control);
        }
    }

    /// <summary>
    /// Sets the position and size of a control based on its grid placement.
    /// </summary>
    private void SetControlPosition(IControl control, int row, int col, int rowspan, int colspan, Alignment alignment)
    {
        int cellWidth = (Width - (_columns - 1) * ColumnSpacing) / _columns;
        int cellHeight = (Height - (_rows - 1) * RowSpacing) / _rows;
        int totalWidth = cellWidth * colspan + ColumnSpacing * (colspan - 1);
        int totalHeight = cellHeight * rowspan + RowSpacing * (rowspan - 1);
        int cellLeft = Left + col * (cellWidth + ColumnSpacing);
        int cellTop = Top + row * (cellHeight + RowSpacing);

        switch (alignment)
        {
            case Alignment.Left:
                control.Left = cellLeft;
                control.Top = cellTop + (totalHeight - control.Height) / 2;
                break;
            case Alignment.Top:
                control.Left = cellLeft + (totalWidth - control.Width) / 2;
                control.Top = cellTop;
                break;
            case Alignment.Right:
                control.Left = cellLeft + totalWidth - control.Width;
                control.Top = cellTop + (totalHeight - control.Height) / 2;
                break;
            case Alignment.Bottom:
                control.Left = cellLeft + (totalWidth - control.Width) / 2;
                control.Top = cellTop + totalHeight - control.Height;
                break;
            case Alignment.Center:
                control.Left = cellLeft + (totalWidth - control.Width) / 2;
                control.Top = cellTop + (totalHeight - control.Height) / 2;
                break;
            case Alignment.Stretch:
                control.Left = cellLeft;
                control.Top = cellTop;
                control.Width = totalWidth;
                control.Height = totalHeight;
                break;
        }
    }

    /// <summary>
    /// Updates the layout of all controls in the grid.
    /// </summary>
    public void LayoutControls()
    {
        foreach (var kvp in _controlPositions)
        {
            var control = kvp.Key;
            var (row, col, rowspan, colspan, alignment) = kvp.Value;
            SetControlPosition(control, row, col, rowspan, colspan, alignment);
        }
    }

    /// <summary>
    /// Draws the grid layout.
    /// </summary>
    protected override void OnDraw(MicroGraphics graphics)
    {
        if (IsVisible && BackgroundColor != null)
        {
            graphics.DrawRectangle(Left, Top, Width, Height, BackgroundColor.Value, true);
        }
    }
}
