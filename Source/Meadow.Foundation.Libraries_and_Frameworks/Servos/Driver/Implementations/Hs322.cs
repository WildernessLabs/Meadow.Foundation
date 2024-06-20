using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Servos;

/// <summary>
/// Represents an HS322 angular servo.
/// </summary>
public class Hs322 : AngularServo
{
    private const double frequencyHz = 50;
    private const double minimumAngle = -90;
    private const double maximumAngle = 90;
    private const double minDurationMs = 0.2;
    private const double maxDurationMs = 3.1;

    /// <summary>
    /// Initializes a new instance of the <see cref="Hs322"/> class with a specified PWM port.
    /// </summary>
    /// <param name="pwm">The PWM port to control the servo.</param>
    public Hs322(IPwmPort pwm)
        : base(pwm,
            new PulseAngle(new Angle(minimumAngle), TimeSpan.FromMilliseconds(minDurationMs)),
            new PulseAngle(new Angle(maximumAngle), TimeSpan.FromMilliseconds(maxDurationMs)))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Hs322"/> class with a specified PWM port.
    /// </summary>
    /// <param name="pwmPin">The PWM pin to control the servo.</param>
    public Hs322(IPin pwmPin)
        : base(pwmPin,
            new Frequency(frequencyHz, Frequency.UnitType.Hertz),
            new PulseAngle(new Angle(minimumAngle), TimeSpan.FromMilliseconds(minDurationMs)),
            new PulseAngle(new Angle(maximumAngle), TimeSpan.FromMilliseconds(maxDurationMs)))
    {
    }
}
