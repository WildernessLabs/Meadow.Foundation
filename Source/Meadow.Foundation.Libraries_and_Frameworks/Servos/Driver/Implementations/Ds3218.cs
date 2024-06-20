using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Servos;

/// <summary>
/// Represents an DS3218 angular servo.
/// </summary>
public class Ds3218 : AngularServo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Ds3218"/> class with a specified PWM port.
    /// </summary>
    /// <param name="pwm">The PWM port to control the servo.</param>
    public Ds3218(IPwmPort pwm)
        : base(pwm,
            new PulseAngle(new Angle(-90), TimeSpan.FromMilliseconds(0.8)),
            new PulseAngle(new Angle(90), TimeSpan.FromMilliseconds(2.2)))
    {
    }
}
