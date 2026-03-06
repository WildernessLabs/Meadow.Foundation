using System;
using System.Threading.Tasks;

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

    /// <summary>
    /// Occurs when the clickable control is long-clicked (pressed and held).
    /// </summary>
    public event EventHandler LongClicked = default!;

    private bool _pressed = false;
    private DateTime? _pressStartTime;
    private TimeSpan _longClickDuration = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// When false, click and long-click events are suppressed and the control does not respond to touch input.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the duration a press must be held to trigger a long click event.
    /// Default is 500 milliseconds.
    /// </summary>
    public TimeSpan LongClickDuration
    {
        get => _longClickDuration;
        set
        {
            if (value < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "LongClickDuration cannot be negative.");
            }
            _longClickDuration = value;
        }
    }

    /// <summary>
    /// Cycles the Control through the pressed and unpressed state, firing the Clicked event
    /// </summary>
    public void Click()
    {
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the clickable control is in a pressed state.
    /// </summary>
    public bool Pressed
    {
        get => _pressed;
        set
        {
            if (!IsVisible || !IsEnabled) { return; }

            if (_pressed == value) { return; }

            _pressed = value;

            if (_pressed)
            {
                // Record the time when the button is pressed
                _pressStartTime = DateTime.UtcNow;
            }
            else
            {
                // Only calculate duration if we have a valid press start time
                if (_pressStartTime.HasValue)
                {
                    // Calculate the press duration
                    var pressDuration = DateTime.UtcNow - _pressStartTime.Value;

                    // Determine if it's a long click or a regular click
                    if (pressDuration >= LongClickDuration)
                    {
                        LongClicked?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        Clicked?.Invoke(this, EventArgs.Empty);
                    }
                }
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
