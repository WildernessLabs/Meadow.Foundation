using System;

namespace Meadow.Foundation.Servos;

/// <summary>
/// Servo motor abstraction
/// </summary>
public interface IServo
{
    /// <summary>
    /// The servo configuration
    /// </summary>
    ServoConfig Config { get; }

    /// <summary>
    /// A trim offset TimeSpan to add to pulse durations to account from hardware variance
    /// </summary>
    TimeSpan TrimDuration { get; set; }
}