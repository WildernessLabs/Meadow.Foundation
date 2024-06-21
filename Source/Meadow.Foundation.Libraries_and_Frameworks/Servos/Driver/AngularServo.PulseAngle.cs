using Meadow.Units;
using System;

namespace Meadow.Foundation.Servos;

/// <summary>
/// Represents an angular servo with specific pulse angles for control.
/// </summary>
public partial class AngularServo
{
    /// <summary>
    /// Represents a mapping between an angle and a pulse width.
    /// </summary>
    public struct PulseAngle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PulseAngle"/> struct with a specified angle and pulse width.
        /// </summary>
        /// <param name="angle">The angle corresponding to the pulse width.</param>
        /// <param name="pulseWidth">The pulse width corresponding to the angle.</param>
        public PulseAngle(Angle angle, TimeSpan pulseWidth)
        {
            Angle = angle;
            PulseWidth = pulseWidth;
        }

        /// <summary>
        /// Gets or sets the pulse width corresponding to the angle.
        /// </summary>
        public TimeSpan PulseWidth { get; set; }

        /// <summary>
        /// Gets or sets the angle corresponding to the pulse width.
        /// </summary>
        public Angle Angle { get; set; }
    }
}
