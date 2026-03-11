namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a container that holds a collection of controls.
/// </summary>
/// <remarks>This interface extends <see cref="IControl"/> to provide functionality for managing child controls.
/// Implementations of this interface are expected to provide access to the contained controls through the <see
/// cref="Controls"/> property.</remarks>
public interface IControlContainer : IControl
{
    /// <summary>
    /// Gets the collection of child controls contained within this control container.
    /// </summary>
    /// <remarks>Use this property to access, enumerate, or manipulate the controls that are direct children
    /// of the container. The returned collection reflects the current set of child controls and can be used to add,
    /// remove, or inspect contained controls.</remarks>
    ControlsCollection Controls { get; }
}
