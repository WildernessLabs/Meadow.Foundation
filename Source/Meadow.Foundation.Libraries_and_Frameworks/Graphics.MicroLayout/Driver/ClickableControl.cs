using System;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a base class for clickable display controls in the user interface.
/// </summary>
public abstract class ClickableControl : ThemedControl, IClickableControl
{
    /// <summary>
    /// Occurs when the clickable control is clicked.
    /// </summary>
    public event EventHandler Clicked = default!;

    private bool _pressed = false;

    /// <summary>
    /// Gets or sets a value indicating whether the clickable control is in a pressed state.
    /// </summary>
    public bool Pressed
    {
        get => _pressed;
        set
        {
            if (!Visible) return;

            if (_pressed == value) return;

            _pressed = value;

            if (!Pressed)
            {
                Clicked?.Invoke(this, EventArgs.Empty);
            }

            // Mark the control as invalid, requiring a redraw.
            Invalidate();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClickableControl"/> class with default dimensions (10x10).
    /// </summary>
    public ClickableControl()
        : base(0, 0, 10, 10)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClickableControl"/> class with the specified dimensions.
    /// </summary>
    /// <param name="left">The left coordinate of the control.</param>
    /// <param name="top">The top coordinate of the control.</param>
    /// <param name="width">The width of the control.</param>
    /// <param name="height">The height of the control.</param>
    public ClickableControl(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
    }
}
