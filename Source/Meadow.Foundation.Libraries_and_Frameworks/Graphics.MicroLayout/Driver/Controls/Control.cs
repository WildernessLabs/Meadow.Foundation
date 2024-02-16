namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a base class for display controls in the user interface.
/// </summary>
public abstract class Control : IControl
{
    private int _left;
    private int _top;
    private int _width;
    private int _height;
    private bool _isVisible = true;

    /// <summary>
    /// Gets or sets a value indicating whether the control needs to be redrawn.
    /// </summary>
    public virtual bool IsInvalid { get; private set; }

    /// <summary>
    /// Gets or sets the context object associated with the control.
    /// </summary>
    public object? Context { get; set; }

    /// <inheritdoc/>
    public IControl? Parent { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Control"/> class with the specified dimensions.
    /// </summary>
    /// <param name="left">The left coordinate of the control.</param>
    /// <param name="top">The top coordinate of the control.</param>
    /// <param name="width">The width of the control.</param>
    /// <param name="height">The height of the control.</param>
    public Control(int left, int top, int width, int height)
    {
        Left = left;
        Top = top;
        Width = width;
        Height = height;

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
        Invalidate();
    }

    /// <summary>
    /// Marks the control as invalid, requiring a redraw.
    /// </summary>
    public virtual void Invalidate()
    {
        IsInvalid = true;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the control is visible.
    /// </summary>
    public virtual bool IsVisible
    {
        get => _isVisible;
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
}
