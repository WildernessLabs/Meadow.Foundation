using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a collection of display controls on a <see cref="DisplayScreen"/>.
/// </summary>
public sealed class ControlsCollection : IEnumerable<IControl>
{
    private readonly DisplayScreen _screen;
    private readonly List<IControl> _controls = new();
    private readonly object _syncRoot = new();

    private readonly IControl? _parent;

    /// <summary>
    /// Initializes a new instance of the <see cref="ControlsCollection"/> class.
    /// </summary>
    /// <param name="screen">The <see cref="DisplayScreen"/> that owns the controls collection.</param>
    /// <param name="parent">The parent control (if exists)</param>
    internal ControlsCollection(DisplayScreen screen, IControl? parent)
    {
        _screen = screen;
        _parent = parent;
    }

    internal object SyncRoot => _syncRoot;

    /// <summary>
    /// Gets a control from the Controls collection by index
    /// </summary>
    /// <param name="index">index of the control to retrieve</param>
    public IControl this[int index]
    {
        get => _controls[index];
    }

    /// <summary>
    /// Removes all display controls from the collection.
    /// </summary>
    public void Clear()
    {
        lock (SyncRoot)
        {
            _controls.Clear();
            _screen.Invalidate();
        }
    }

    /// <summary>
    /// Gets the number of display controls in the collection.
    /// </summary>
    public int Count => _controls.Count;

    /// <summary>
    /// Adds one or more display controls to the collection.
    /// </summary>
    /// <param name="controls">The display controls to add.</param>
    public void Add(params IControl[] controls)
    {
        // Apply screen theme to the added controls, if available.
        if (_screen.Theme != null)
        {
            foreach (IThemedControl control in controls)
            {
                control.ApplyTheme(_screen.Theme);
            }
        }

        lock (SyncRoot)
        {
            foreach (var control in controls)
            {
                control.Parent = _parent;
                _controls.Add(control);
            }
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection of display controls.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<IControl> GetEnumerator()
    {
        return _controls.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection of display controls.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
