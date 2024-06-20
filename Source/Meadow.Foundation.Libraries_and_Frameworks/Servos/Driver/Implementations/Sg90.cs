using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Servos;

/// <summary>
/// Represents an SG90 angular servo.
/// </summary>
public class Sg90 : AngularServo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Sg90"/> class with a specified PWM port.
    /// </summary>
    /// <param name="pwm">The PWM port to control the servo.</param>
    public Sg90(IPwmPort pwm)
        : base(pwm,
            new PulseAngle(new Angle(-90), TimeSpan.FromMilliseconds(0.2)),
            new PulseAngle(new Angle(90), TimeSpan.FromMilliseconds(3.1)))
    {
    }
}
