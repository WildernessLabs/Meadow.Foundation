using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A layout that arranges child controls in a grid.
/// </summary>
public class GridLayout : MicroLayout
{
    private readonly int _rows;
    private readonly int _columns;
    private readonly Dictionary<IControl, (int row, int col)> _controlPositions = new();

    public int RowSpacing { get; set; } = 2;
    public int ColumnSpacing { get; set; } = 2;

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

    public void Add(IControl control, int row, int col)
    {
        if (row < 0 || row >= _rows || col < 0 || col >= _columns)
        {
            throw new ArgumentOutOfRangeException("Row or column is out of range.");
        }

        Controls.Add(control);
        SetControlPosition(control, row, col);
    }

    private void SetControlPosition(IControl control, int row, int col)
    {
        int cellWidth = (Width - (_columns - 1) * ColumnSpacing) / _columns;
        int cellHeight = (Height - (_rows - 1) * RowSpacing) / _rows;

        control.Left = Left + col * (cellWidth + ColumnSpacing);
        control.Top = Top + row * (cellHeight + RowSpacing);
        control.Width = cellWidth;
        control.Height = cellHeight;
    }

    public void UpdateLayout()
    {
        foreach (var kvp in _controlPositions)
        {
            var control = kvp.Key;
            var (row, col) = kvp.Value;

            SetControlPosition(control, row, col);
        }
    }

    protected override void OnDraw(MicroGraphics graphics)
    {
        if (IsVisible && BackgroundColor != null)
        {
            graphics.DrawRectangle(Left, Top, Width, Height, BackgroundColor.Value, true);
        }
    }
}