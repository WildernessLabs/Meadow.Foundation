using Meadow.Peripherals;

namespace Meadow.Foundation.Servos;

/// <summary>
/// Represents a continuous rotation servo interface, extending the basic servo interface with additional properties and methods for controlling rotation.
/// </summary>
public interface IContinuousRotationServo : IServo
{
    /// <summary>
    /// Gets the current direction of rotation.
    /// </summary>
    RotationDirection Direction { get; }

    /// <summary>
    /// Gets the current speed of rotation, between 0.0 (stopped) and 1.0 (full speed).
    /// </summary>
    double Speed { get; }

    /// <summary>
    /// Rotates the servo in the specified direction at the specified speed.
    /// </summary>
    /// <param name="direction">The direction of rotation.</param>
    /// <param name="speed">The speed of rotation, between 0.0 (stopped) and 1.0 (full speed).</param>
    void Rotate(RotationDirection direction, double speed);
}
