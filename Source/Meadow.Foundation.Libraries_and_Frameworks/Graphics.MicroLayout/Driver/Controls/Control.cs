using System;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a base class for display controls in the user interface.
/// </summary>
public abstract class Control : IControl
{
    /// <summary>
    /// Occurs when the position or size of the control changes.
    /// </summary>
    public event EventHandler? BoundsChanged;

    /// <summary>
    /// Gets or sets a value indicating whether the control is visible.
    /// </summary>
    public virtual bool IsVisible
    {
        get => _isVisible && (_parent?.IsVisible ?? true);
        set => SetInvalidatingProperty(ref _isVisible, value);
    }

    /// <summary>
    /// Gets or sets the left coordinate of the control.
    /// </summary>
    public virtual int Left
    {
        get => _left;
        set => SetInvalidatingProperty(ref _left, value);
    }

    /// <summary>
    /// Gets or sets the top coordinate of the control.
    /// </summary>
    public virtual int Top
    {
        get => _top;
        set => SetInvalidatingProperty(ref _top, value);
    }

    /// <summary>
    /// Gets or sets the width of the control.
    /// </summary>
    public virtual int Width
    {
        get => _width;
        set => SetInvalidatingProperty(ref _width, value);
    }

    /// <summary>
    /// Gets or sets the height of the control.
    /// </summary>
    public virtual int Height
    {
        get => _height;
        set => SetInvalidatingProperty(ref _height, value);
    }

    /// <summary>
    /// Gets the bottom coordinate of the control.
    /// </summary>
    public virtual int Bottom => Top + Height;

    /// <summary>
    /// Gets the right coordinate of the control.
    /// </summary>
    public virtual int Right => Left + Width;

    /// <inheritdoc />
    public int ScreenLeft => Left + (_parent?.ScreenLeft ?? 0);

    /// <inheritdoc />
    public int ScreenTop => Top + (_parent?.ScreenTop ?? 0);

    /// <inheritdoc />
    public int ScreenRight => ScreenLeft + Width;

    /// <inheritdoc />
    public int ScreenBottom => ScreenTop + Height;

    private int _left;
    private int _top;
    private int _width;
    private int _height;
    private bool _isVisible = true;
    private IControl? _parent;

    /// <summary>
    /// Gets or sets a value indicating whether the control needs to be redrawn.
    /// </summary>
    public virtual bool IsInvalid { get; private set; }

    /// <summary>
    /// Gets or sets the context object associated with the control.
    /// </summary>
    public object? Context { get; set; }

    /// <inheritdoc/>
    public virtual IControl? Parent
    {
        get => _parent;
        set
        {
            if (_parent == value) return;

            if (_parent != null)
            {
                _parent.BoundsChanged -= OnParentBoundsChanged;
            }

            _parent = value;

            if (_parent != null)
            {
                _parent.BoundsChanged += OnParentBoundsChanged;
            }

            BoundsChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Control"/> class with the specified dimensions.
    /// </summary>
    public Control() : this(0, 0, 0, 0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Control"/> class with the specified dimensions.
    /// </summary>
    /// <param name="left">The left coordinate of the control.</param>
    /// <param name="top">The top coordinate of the control.</param>
    /// <param name="width">The width of the control.</param>
    /// <param name="height">The height of the control.</param>
    public Control(int left, int top, int width, int height)
    {
        _left = left;
        _top = top;
        _width = width;
        _height = height;

        IsInvalid = true;
    }

    /// <summary>
    /// Sets a property value and marks the control as invalid, requiring a redraw.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">The reference to the field to set.</param>
    /// <param name="value">The new value to assign to the property.</param>
    protected void SetInvalidatingProperty<T>(ref T field, T value)
    {
        if (field != null && field.Equals(value))
        {
            // do nothing if they are equal
            return;
        }

        field = value;

        BoundsChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    /// <summary>
    /// Marks the control as invalid, requiring a redraw.
    /// </summary>
    public virtual void Invalidate()
    {
        IsInvalid = true;

        // Walk up parent chain to notify DisplayScreen
        var current = Parent;
        while (current != null)
        {
            if (current is DisplayScreen screen)
            {
                screen.NotifyControlInvalidated();
                break;
            }
            current = current.Parent;
        }
    }

    /// <summary>
    /// Refreshes the control by redrawing it on the specified <see cref="MicroGraphics"/> surface.
    /// </summary>
    /// <param name="graphics">The <see cref="MicroGraphics"/> surface to redraw the control on.</param>
    public void Refresh(MicroGraphics graphics)
    {
        if (IsInvalid)
        {
            if (IsVisible)
            {
                OnDraw(graphics);
            }
            IsInvalid = false;
        }
    }

    /// <summary>
    /// Performs the actual drawing of the control on the specified <see cref="MicroGraphics"/> surface.
    /// This method must be implemented in derived classes.
    /// </summary>
    /// <param name="graphics">The <see cref="MicroGraphics"/> surface to draw the control on.</param>
    protected abstract void OnDraw(MicroGraphics graphics);

    /// <summary>
    /// Handles changes to the bounds of the parent control and triggers a layout update for the current control.
    /// </summary>
    /// <remarks>This method is typically invoked when the parent control's size or position changes. Derived
    /// classes can override this method to implement custom layout behavior based on the parent's bounds.</remarks>
    /// <param name="sender">The source of the event, typically the parent control.</param>
    /// <param name="e">An <see cref="EventArgs"/> instance containing event data.</param>
    protected virtual void OnParentBoundsChanged(object? sender, EventArgs e)
    {
        // This is where you'd trigger a re-layout of the child
        Invalidate();
    }
}