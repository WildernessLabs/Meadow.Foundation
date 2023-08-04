using Meadow.Foundation.Graphics;

namespace Meadow.Foundation.Displays.UI;

/// <summary>
/// Represents a base class for display controls in the user interface.
/// </summary>
public abstract class DisplayControl : IDisplayControl
{
    private int _left;
    private int _top;
    private int _width;
    private int _height;
    private bool _visible = true;

    /// <summary>
    /// Gets or sets a value indicating whether the control needs to be redrawn.
    /// </summary>
    public bool IsInvalid { get; private set; }

    /// <summary>
    /// Gets or sets the context object associated with the control.
    /// </summary>
    public object? Context { get; set; }

    /// <summary>
    /// Applies the specified display theme to the control.
    /// </summary>
    /// <param name="theme">The display theme to apply.</param>
    public abstract void ApplyTheme(DisplayTheme theme);

    /// <summary>
    /// Initializes a new instance of the <see cref="DisplayControl"/> class with the specified dimensions.
    /// </summary>
    /// <param name="left">The left coordinate of the control.</param>
    /// <param name="top">The top coordinate of the control.</param>
    /// <param name="width">The width of the control.</param>
    /// <param name="height">The height of the control.</param>
    public DisplayControl(int left, int top, int width, int height)
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
        field = value;
        Invalidate();
    }

    /// <summary>
    /// Marks the control as invalid, requiring a redraw.
    /// </summary>
    public void Invalidate()
    {
        IsInvalid = true;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the control is visible.
    /// </summary>
    public bool Visible
    {
        get => _visible;
        set => SetInvalidatingProperty(ref _visible, value);
    }

    /// <summary>
    /// Gets or sets the left coordinate of the control.
    /// </summary>
    public int Left
    {
        get => _left;
        set => SetInvalidatingProperty(ref _left, value);
    }

    /// <summary>
    /// Gets or sets the top coordinate of the control.
    /// </summary>
    public int Top
    {
        get => _top;
        set => SetInvalidatingProperty(ref _top, value);
    }

    /// <summary>
    /// Gets or sets the width of the control.
    /// </summary>
    public int Width
    {
        get => _width;
        set => SetInvalidatingProperty(ref _width, value);
    }

    /// <summary>
    /// Gets or sets the height of the control.
    /// </summary>
    public int Height
    {
        get => _height;
        set => SetInvalidatingProperty(ref _height, value);
    }

    /// <summary>
    /// Gets the bottom coordinate of the control.
    /// </summary>
    public int Bottom => Top + Height;

    /// <summary>
    /// Gets the right coordinate of the control.
    /// </summary>
    public int Right => Left + Width;

    /// <summary>
    /// Refreshes the control by redrawing it on the specified <see cref="MicroGraphics"/> surface.
    /// </summary>
    /// <param name="graphics">The <see cref="MicroGraphics"/> surface to redraw the control on.</param>
    public void Refresh(MicroGraphics graphics)
    {
        if (IsInvalid)
        {
            if (Visible)
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
