using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Servos;

/// <summary>
/// Represents an MG996 angular servo.
/// </summary>
public class Mg996 : AngularServo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Mg996"/> class with a specified PWM port.
    /// </summary>
    /// <param name="pwm">The PWM port to control the servo.</param>
    public Mg996(IPwmPort pwm)
        : base(pwm,
            new PulseAngle(new Angle(-90), TimeSpan.FromMilliseconds(0.8)),
            new PulseAngle(new Angle(90), TimeSpan.FromMilliseconds(2.2)))
    {
    }
}
