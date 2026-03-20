using System;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a control that can be selected for user interaction.
/// </summary>
public interface ISelectable
{
    /// <summary>
    /// Gets or sets whether the control can be selected.
    /// </summary>
    bool IsSelectable { get; set; }

    /// <summary>
    /// Gets or sets whether the control is currently selected.
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets the selection index used for navigation ordering.
    /// Controls with lower indices are selected first.
    /// </summary>
    int SelectionIndex { get; set; }

    /// <summary>
    /// Gets or sets the action to perform when rendering the control in selected state.
    /// If null, the default selection rendering (color inversion) is used.
    /// </summary>
    Action<MicroGraphics, ISelectable>? SelectionAction { get; set; }
}
