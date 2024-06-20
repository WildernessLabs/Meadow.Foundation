using System;

namespace Meadow.Foundation.Servos;

/// <summary>
/// Represents a servo motor interface with properties for configuration.
/// </summary>
public interface IServo
{
    /// <summary>
    /// Gets or sets the trim offset for the servo.
    /// The trim offset is used to adjust the neutral position of the servo.
    /// </summary>
    TimeSpan TrimOffset { get; set; }

    /// <summary>
    /// Sets the servo to its neutral position.
    /// </summary>
    void Neutral();

    /// <summary>
    /// Disables the servo, stopping any movement.
    /// </summary>
    void Disable();
}