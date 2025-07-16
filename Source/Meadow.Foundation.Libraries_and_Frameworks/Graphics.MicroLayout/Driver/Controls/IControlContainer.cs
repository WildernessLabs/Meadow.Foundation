namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a container that holds a collection of controls.
/// </summary>
/// <remarks>This interface extends <see cref="IControl"/> to provide functionality for managing child controls.
/// Implementations of this interface are expected to provide access to the contained controls through the <see
/// cref="Controls"/> property.</remarks>
public interface IControlContainer : IControl
{
    ControlsCollection Controls { get; }
}
