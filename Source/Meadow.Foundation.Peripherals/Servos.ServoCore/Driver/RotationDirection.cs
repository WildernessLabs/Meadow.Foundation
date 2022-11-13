using System;

namespace Meadow.Foundation.Servos
{
    /// <summary>
    /// Describes the direction of rotation for a servo
    /// </summary>
    public enum RotationDirection
    {
        /// <summary>
        /// Clockwise
        /// </summary>
        Clockwise,
        /// <summary>
        /// Counter-clockwise
        /// </summary>
        CounterClockwise,
        /// <summary>
        /// None / stopped
        /// </summary>
        None
    }
}