using Meadow.Foundation.Graphics.MicroLayout;
using static Meadow.Foundation.Graphics.MicroLayout.GridLayout;

namespace Graphics.MicroLayout;

/// <summary>
/// Represents a collection of controls specifically designed for grid-based layouts.
/// </summary>
/// <remarks>This collection provides methods to add controls with specific positioning and alignment within a
/// grid. It extends the functionality of <see cref="ControlsCollection"/> by supporting grid-related properties such as
/// row, column, rowspan, colspan, and alignment.</remarks>
public class GridControlsCollection : ControlsCollection
{
    internal GridControlsCollection(IControlContainer? parent) : base(parent)
    { }

    /// <inheritdoc/>
    public new void Add(params IControl[] controls)
    {
        foreach (IControl control in controls)
        {
            Add(control, 0, 0, 1, 1, Alignment.Center);
        }
    }

    /// <summary>
    /// Adds one or more display controls to the collection.
    /// </summary>
    /// <param name="control">The control to add.</param>
    /// <param name="row">The row index of the control.</param>
    /// <param name="col">The column index of the control.</param>
    /// <param name="rowspan">The number of rows the control spans.</param>
    /// <param name="colspan">The number of columns the control spans.</param>
    /// <param name="alignment">The alignment of the control within the cell.</param>
    public void Add(IControl control, int row, int col, int rowspan = 1, int colspan = 1, Alignment alignment = Alignment.Center)
    {
        // Apply screen theme to the added controls, if available.
        if (_container is DisplayScreen screen)
        {
            if (control is IThemedControl && screen.Theme != null)
            {
                ((IThemedControl)control).ApplyTheme(screen.Theme);
            }
        }

        try
        {
            lock (SyncRoot)
            {
                if (control is null) { return; }

                control.Parent = _container;
                control.Invalidate();
                _controls.Add(control);
            }
        }
        finally
        {
            if (control is not null)
            {
                OnControlAdded(control);
            }
        }
    }
}