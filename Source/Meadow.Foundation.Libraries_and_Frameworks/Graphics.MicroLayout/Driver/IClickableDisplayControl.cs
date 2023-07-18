using System;

namespace Meadow.Foundation.Displays.UI;

/// <summary>
/// Represents a clickable display control in the user interface.
/// </summary>
public interface IClickableDisplayControl : IDisplayControl
{
    /// <summary>
    /// Occurs when the clickable display control is clicked.
    /// </summary>
    event EventHandler Clicked;

    /// <summary>
    /// Gets or sets a value indicating whether the clickable display control is currently pressed.
    /// </summary>
    bool Pressed { get; set; }
}
