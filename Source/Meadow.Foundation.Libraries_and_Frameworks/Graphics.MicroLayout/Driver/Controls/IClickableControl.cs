using System;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a clickable display control in the user interface.
/// </summary>
public interface IClickableControl : IControl
{
    /// <summary>
    /// Occurs when the clickable display control is clicked.
    /// </summary>
    event EventHandler Clicked;

    /// <summary>
    /// Occurs when the clickable display control is long-clicked (pressed and held).
    /// </summary>
    event EventHandler LongClicked;

    /// <summary>
    /// Gets or sets a value indicating whether the clickable display control is currently pressed.
    /// </summary>
    bool Pressed { get; set; }

    /// <summary>
    /// Gets or sets the duration a press must be held to trigger a long click event.
    /// </summary>
    TimeSpan LongClickDuration { get; set; }
}
