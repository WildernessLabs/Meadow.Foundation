using System;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a collection of display controls on a <see cref="DisplayScreen"/>.
/// </summary>
public class ControlsCollection : IEnumerable<IControl>
{
    /// <summary>
    /// Occurs when a control is added to the collection.
    /// </summary>
    public event EventHandler<IControl>? ControlAdded;

    /// <summary>
    /// Occurs when a control is removed from the collection.
    /// </summary>
    public event EventHandler<IControl>? ControlRemoved;

    internal readonly List<IControl> _controls = new();
    private readonly object _syncRoot = new();

    /// <summary>
    /// The control container that owns this collection, if it exists.
    /// </summary>
    internal readonly IControlContainer? _container;

    /// <summary>
    /// Initializes a new instance of the <see cref="ControlsCollection"/> class.
    /// </summary>
    /// <param name="parent">The parent control (if exists)</param>
    internal ControlsCollection(IControlContainer? parent)
    {
        _container = parent;
    }

    internal object SyncRoot => _syncRoot;

    /// <summary>
    /// Gets a control from the Controls collection by index
    /// </summary>
    /// <param name="index">index of the control to retrieve</param>
    public IControl this[int index]
    {
        get
        {
            if (index < 0 || index >= _controls.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return _controls[index];
        }
    }

    /// <summary>
    /// Removes all display controls from the collection.
    /// </summary>
    public void Clear()
    {
        List<IControl> controlsToClear;
        lock (SyncRoot)
        {
            controlsToClear = [.. _controls];
            _controls.Clear();
        }

        foreach (var control in controlsToClear)
        {
            control.Parent = null;
            control.Invalidate();
            OnControlRemoved(control);
        }

        _container?.Parent?.Invalidate();
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
        if (_container is DisplayScreen screen)
        {
            if (screen.Theme != null)
            {
                foreach (IThemedControl control in controls)
                {
                    control.ApplyTheme(screen.Theme);
                }
            }
        }

        try
        {
            lock (SyncRoot)
            {
                foreach (var control in controls)
                {
                    if (control is null) { continue; }

                    if (_controls.Contains(control))
                    {
                        continue;
                    }

                    control.Parent = _container;
                    control.Invalidate();

                    // If the control is a container, recursively invalidate all children
                    // This ensures nested controls are redrawn when re-added to the display
                    if (control is IControlContainer container)
                    {
                        InvalidateControlsRecursive(container);
                    }

                    _controls.Add(control);
                }
            }
        }
        finally
        {
            foreach (var control in controls)
            {
                if (control is null) { continue; }

                ControlAdded?.Invoke(this, control);
            }
        }
    }

    /// <summary>
    /// Removes a control from the collection.
    /// </summary>
    /// <param name="control">The control to remove.</param>
    /// <returns>True if the control was removed; otherwise, false.</returns>
    public bool Remove(IControl control)
    {
        if (control is null) { return false; }

        try
        {
            lock (SyncRoot)
            {
                if (_controls.Remove(control))
                {
                    _container?.Parent?.Invalidate();
                    return true;
                }
            }
            return false;
        }
        finally
        {
            ControlRemoved?.Invoke(this, control);
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

    /// <summary>
    /// Invoked when a control is added to the container.
    /// </summary>
    /// <remarks>This method raises the <see cref="ControlAdded"/> event, allowing subscribers to respond  to
    /// the addition of a new control. Override this method in a derived class to provide  custom behavior when a
    /// control is added.</remarks>
    /// <param name="control">The control that was added. Cannot be null.</param>
    protected virtual void OnControlAdded(IControl control)
    {
        ControlAdded?.Invoke(this, control);
    }

    /// <summary>
    /// Invoked when a control is removed from the current instance.
    /// </summary>
    /// <remarks>This method raises the <see cref="ControlRemoved"/> event to notify subscribers  that a
    /// control has been removed. Override this method in a derived class to  provide custom behavior when a control is
    /// removed, ensuring the base implementation  is called to maintain event invocation.</remarks>
    /// <param name="control">The control that was removed. Cannot be null.</param>
    protected virtual void OnControlRemoved(IControl control)
    {
        ControlRemoved?.Invoke(this, control);
    }

    /// <summary>
    /// Recursively invalidates all controls in a container and its nested containers.
    /// </summary>
    /// <remarks>This ensures that when a container is re-added to the display,
    /// all of its children are marked for redrawing, not just the container itself.</remarks>
    /// <param name="container">The container whose controls should be invalidated.</param>
    private void InvalidateControlsRecursive(IControlContainer container)
    {
        foreach (var child in container.Controls)
        {
            child.Invalidate();

            if (child is IControlContainer childContainer)
            {
                InvalidateControlsRecursive(childContainer);
            }
        }
    }
}