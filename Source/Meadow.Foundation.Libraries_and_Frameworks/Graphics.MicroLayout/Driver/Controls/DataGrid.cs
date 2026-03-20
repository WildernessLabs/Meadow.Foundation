using System;
using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a customizable data grid control for displaying tabular data with support for headers, rows, and columns.
/// </summary>
/// <remarks>The <see cref="DataGrid"/> class provides functionality for defining column headers, adding,
/// inserting, and removing rows, and updating individual cells. It supports customizable appearance, including text and
/// background colors for headers, rows, and alternating rows. The control automatically invalidates and redraws itself
/// when data or appearance properties are modified.</remarks>
public class DataGrid : ThemedControl
{
    private static Color DefaultTextColor = Color.White;
    private static Color DefaultBackgroundColor = Color.Black;
    private static Color DefaultHeaderBackgroundColor = Color.DarkGray;

    private readonly List<object[]> _rows = new();
    private Color? _textColor;
    private Color? _backgroundColor;
    private readonly DisplayTheme? _theme;

    /// <summary>
    /// Represents the definition of a column, including its header, width, and alignment settings.
    /// </summary>
    /// <remarks>This class is used to define the properties of a column, such as its display header, width, 
    /// and alignment. It can be used in scenarios where tabular data or grid-like structures are required.</remarks>
    public class ColumnDefinition
    {
        /// <summary>
        /// Gets or sets the header value associated with the request or response.
        /// </summary>
        public string? Header { get; set; }
        /// <summary>
        /// Gets or sets the width of the element.
        /// </summary>
        public int Width { get; set; } = 50;
        /// <summary>
        /// Gets or sets the horizontal alignment of the content within the container.
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
        /// <summary>
        /// Gets or sets the vertical alignment of the content within its container.
        /// </summary>
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;

        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        public Color? TextColor { get; set; } = null;

        /// <summary>
        /// Represents the definition of a column, including its width and optional header text.
        /// </summary>
        /// <param name="width">The width of the column, specified as an integer. Must be a positive value.</param>
        /// <param name="header">The optional header text for the column. If null, the column will have no header.</param>
        public ColumnDefinition(int width, string? header = null)
        {
            Header = header;
            Width = width;
        }
    }

    /// <summary>
    /// Gets or sets the height, in pixels, of a row in the grid.
    /// </summary>
    public int RowHeight { get; set; } = 20;
    /// <summary>
    /// Gets or sets the height, in pixels, of the header row in the table.
    /// </summary>
    public int HeaderRowHeight { get; set; } = 20;
    /// <summary>
    /// Gets the number of data rows currently stored in the collection.
    /// </summary>
    public int RowCount => _rows.Count;

    /// <summary>
    /// Gets the collection of column definitions for the current layout.
    /// </summary>
    public ColumnDefinition[] Columns { get; }
    /// <summary>
    /// Gets or sets the background color of the header.
    /// </summary>
    public Color HeaderBackgroundColor { get; set; } = DefaultHeaderBackgroundColor;
    /// <summary>
    /// Gets or sets the color of the header text.
    /// </summary>
    public Color HeaderTextColor { get; set; } = DefaultTextColor;
    /// <summary>
    /// Gets or sets the background color for even rows in a table or grid.
    /// </summary>
    public Color? EvenRowBackgroundColor { get; set; } = null;
    /// <summary>
    /// Gets or sets the text color for even rows in a display or grid.
    /// </summary>
    public Color? EvenRowTextColor { get; set; } = null;

    /// <summary>
    /// Gets or sets the font used to render data rows in the table.
    /// </summary>
    public IFont? RowFont { get; set; } = null;
    /// <summary>
    /// Gets or sets the font used for rendering headers.
    /// </summary>
    public IFont? HeaderFont { get; set; } = null;

    /// <summary>
    /// Gets or sets the text color of the label text.
    /// </summary>
    public Color TextColor
    {
        get => _textColor ?? _theme?.TextColor ?? DefaultTextColor;
        set => SetInvalidatingProperty(ref _textColor, value);
    }

    /// <summary>
    /// Gets or sets the background color of the label display control.
    /// </summary>
    public Color BackgroundColor
    {
        get => _backgroundColor ?? _theme?.BackgroundColor ?? DefaultBackgroundColor;
        set => SetInvalidatingProperty(ref _backgroundColor, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGrid"/> class with the specified position, size, and column
    /// definitions.
    /// </summary>
    /// <param name="left">The x-coordinate of the top-left corner of the data grid.</param>
    /// <param name="top">The y-coordinate of the top-left corner of the data grid.</param>
    /// <param name="width">The width of the data grid, in pixels. Must be greater than zero.</param>
    /// <param name="height">The height of the data grid, in pixels. Must be greater than zero.</param>
    /// <param name="columnDefinitions">An array of <see cref="ColumnDefinition"/> objects that define the columns of the data grid. Cannot be null or
    /// empty.</param>
    public DataGrid(int left, int top, int width, int height, ColumnDefinition[] columnDefinitions)
        : base(left, top, width, height)
    {
        Columns = columnDefinitions;
    }

    /// <summary>
    /// Adds a new row to the collection with the specified values.
    /// </summary>
    /// <remarks>If the number of values provided exceeds the number of columns, only the values corresponding
    /// to the existing columns are added. The method triggers a redraw of the affected area to reflect the newly added
    /// row.</remarks>
    /// <param name="values">An array of values to populate the new row. The number of values must not exceed the number of columns.</param>
    public void AddRow(params object[] values)
    {
        lock (_rows)
        {
            object[] rowValues = new object[Columns.Length];
            for (int i = 0; i < Columns.Length; i++)
            {
                rowValues[i] = i < values.Length ? values[i] : string.Empty;
            }

            _rows.Add(rowValues);
        }

        // TODO: clear and draw just the added row area
        this.Invalidate();
    }

    /// <summary>
    /// Inserts a new row at the specified index with the provided values.
    /// </summary>
    /// <remarks>Only the values corresponding to the number of columns will be inserted. Any additional
    /// values in the <paramref name="values"/> array will be ignored. After the row is inserted, the display is
    /// invalidated to reflect the changes.</remarks>
    /// <param name="index">The zero-based index at which the new row should be inserted. Must be within the range of existing rows.</param>
    /// <param name="values">An array of values to populate the new row. The number of values must not exceed the number of columns.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than the current number of rows.</exception>
    public void InsertRow(int index, params object[] values)
    {
        lock (_rows)
        {
            if (index < 0 || index > _rows.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
            _rows.Insert(index, values[..Columns.Length]);
        }

        // TODO: clear and redraw the inserted row and everything after it
        this.Invalidate();
    }

    /// <summary>
    /// Removes the row at the specified index from the collection.
    /// </summary>
    /// <remarks>After the row is removed, subsequent rows are shifted to fill the gap. The method also
    /// invalidates the control to trigger a redraw.</remarks>
    /// <param name="index">The zero-based index of the row to remove. Must be within the valid range of the collection.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than or equal to the number of rows in the
    /// collection.</exception>
    public void RemoveRow(int index)
    {
        lock (_rows)
        {
            if (index < 0 || index >= _rows.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
            _rows.RemoveAt(index);
        }
        // TODO: clear and redraw the removed row and everything after it

        this.Invalidate();
    }

    /// <summary>
    /// Updates the value of a specific cell in the grid.
    /// </summary>
    /// <remarks>After updating the cell value, the grid is invalidated to ensure the updated cell is
    /// redrawn.</remarks>
    /// <param name="rowIndex">The zero-based index of the row containing the cell to update. Must be within the valid range of rows.</param>
    /// <param name="columnIndex">The zero-based index of the column containing the cell to update. Must be within the valid range of columns.</param>
    /// <param name="value">The new value to assign to the specified cell.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="rowIndex"/> is less than 0, greater than or equal to the number of rows,  or if
    /// <paramref name="columnIndex"/> is less than 0 or greater than or equal to the number of columns.</exception>
    public void UpdateCell(int rowIndex, int columnIndex, object value)
    {
        lock (_rows)
        {
            if (rowIndex < 0 || rowIndex >= _rows.Count || columnIndex < 0 || columnIndex >= Columns.Length)
            {
                throw new ArgumentOutOfRangeException("Row or column index is out of range.");
            }

            if (_rows[rowIndex][columnIndex] != value)
            {
                _rows[rowIndex][columnIndex] = value;

                // TODO: clear and draw just the cell area

                this.Invalidate();
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnDraw(MicroGraphics graphics)
    {
        int x = ScreenLeft;
        int y = ScreenTop;

        if (this.IsInvalid)
        {
            if (BackgroundColor != Color.Transparent)
            {
                graphics.DrawRectangle(ScreenLeft, ScreenTop, Width, Height, BackgroundColor, true);
            }

            if (Columns.Any(c => c.Header != null))
            {
                DrawHeaderRow(graphics, x, y);

                y += HeaderRowHeight;
            }

            var r = 0;

            Color rowBackgroundColor;
            Color rowTextColor;

            lock (_rows)
            {
                foreach (var row in _rows)
                {
                    if (++r % 2 == 0)
                    {
                        rowBackgroundColor = EvenRowBackgroundColor ?? BackgroundColor;
                        rowTextColor = EvenRowTextColor ?? TextColor;
                    }
                    else
                    {
                        rowBackgroundColor = BackgroundColor;
                        rowTextColor = TextColor;
                    }

                    DrawRow(graphics,
                            rowBackgroundColor,
                            rowTextColor,
                            x,
                            y,
                            row);
                    y += RowHeight;
                }
            }
            graphics.Show();
        }
    }

    private void DrawHeaderRow(MicroGraphics graphics, int x, int y)
    {
        for (int i = 0; i < Columns.Length; i++)
        {
            var column = Columns[i];
            var value = column.Header ?? string.Empty;

            // Draw row background
            graphics.DrawRectangle(x, y, column.Width, HeaderRowHeight, HeaderBackgroundColor, true);

            // TODO: cell borders?

            // Draw row content
            graphics.DrawText(
                x + 2, // TODO: padding?
                y + 2, // TODO: padding?
                value,
                HeaderTextColor,
                alignmentH: column.HorizontalAlignment,
                alignmentV: column.VerticalAlignment,
                font: HeaderFont);
            x += column.Width;
        }
    }

    private void DrawRow(MicroGraphics graphics, Color backgroundColor, Color textColor, int x, int y, object[] values)
    {
        for (int i = 0; i < Columns.Length; i++)
        {
            var column = Columns[i];
            var value = values[i]?.ToString() ?? string.Empty;
            // TODO: alternating back color?
            // Draw cell background
            graphics.DrawRectangle(x, y, column.Width, RowHeight, backgroundColor, true);

            // TODO: cell borders?
            //graphics.Stroke = 1;
            //graphics.DrawRectangle(x, y, column.Width, RowHeight, BorderColor, false);

            // Draw cell content
            graphics.DrawText(
                x + 2, // TODO: padding?
                y + 2, // TODO: padding?
                value,
                column.TextColor ?? textColor,
                alignmentH: column.HorizontalAlignment,
                alignmentV: column.VerticalAlignment,
                font: RowFont);
            x += column.Width;


        }
    }
}
