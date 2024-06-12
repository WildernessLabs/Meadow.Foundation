namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A container for a collection of controls
/// </summary>
public interface IControlContainer
{
    /// <summary>
    /// The collection's parent control (if it exists)
    /// </summary>
    IControl? Parent { get; }
    /// <summary>
    /// the controls in the collection
    /// </summary>
    ControlsCollection Controls { get; }
    /// <summary>
    /// Gets or sets the visiblity of the collection of controls
    /// </summary>
    bool IsVisible { get; set; }
    /// <summary>
    /// Invalidates all controls in the container
    /// </summary>
    void Invalidate();
}
