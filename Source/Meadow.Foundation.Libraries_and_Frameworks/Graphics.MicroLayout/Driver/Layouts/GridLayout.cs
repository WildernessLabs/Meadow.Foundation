using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A layout that arranges child controls in a grid with alignment options.
/// </summary>
public class GridLayout : MicroLayout
{
    public enum Alignment
    {
        Left,
        Top,
        Right,
        Bottom,
        Center,
        Stretch
    }

    private readonly int _rows;
    private readonly int _columns;
    private readonly Dictionary<IControl, (int row, int col, Alignment alignment)> _controlPositions = new();

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

    public void Add(IControl control, int row, int col, Alignment alignment = Alignment.Center)
    {
        if (row < 0 || row >= _rows || col < 0 || col >= _columns)
        {
            throw new ArgumentOutOfRangeException("Row or column is out of range.");
        }

        Controls.Add(control);
        _controlPositions[control] = (row, col, alignment);
        SetControlPosition(control, row, col, alignment);
    }

    private void SetControlPosition(IControl control, int row, int col, Alignment alignment)
    {
        int cellWidth = (Width - (_columns - 1) * ColumnSpacing) / _columns;
        int cellHeight = (Height - (_rows - 1) * RowSpacing) / _rows;
        int cellLeft = Left + col * (cellWidth + ColumnSpacing);
        int cellTop = Top + row * (cellHeight + RowSpacing);

        switch (alignment)
        {
            case Alignment.Left:
                control.Left = cellLeft;
                control.Top = cellTop + (cellHeight - control.Height) / 2;
                break;
            case Alignment.Top:
                control.Left = cellLeft + (cellWidth - control.Width) / 2;
                control.Top = cellTop;
                break;
            case Alignment.Right:
                control.Left = cellLeft + cellWidth - control.Width;
                control.Top = cellTop + (cellHeight - control.Height) / 2;
                break;
            case Alignment.Bottom:
                control.Left = cellLeft + (cellWidth - control.Width) / 2;
                control.Top = cellTop + cellHeight - control.Height;
                break;
            case Alignment.Center:
                control.Left = cellLeft + (cellWidth - control.Width) / 2;
                control.Top = cellTop + (cellHeight - control.Height) / 2;
                break;
            case Alignment.Stretch:
                control.Left = cellLeft;
                control.Top = cellTop;
                control.Width = cellWidth;
                control.Height = cellHeight;
                break;
        }
    }

    public void UpdateLayout()
    {
        foreach (var kvp in _controlPositions)
        {
            var control = kvp.Key;
            var (row, col, alignment) = kvp.Value;
            SetControlPosition(control, row, col, alignment);
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
