using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Servos;

/// <summary>
/// Represents a Miuzei MS24 angular servo.
/// </summary>
public class Ms24 : AngularServo
{
    private const double frequencyHz = 50;
    private const double minimumAngle = -235;
    private const double maximumAngle = 235;
    private const double minDurationMs = 0.5;
    private const double maxDurationMs = 2.5;

    /// <summary>
    /// Initializes a new instance of the <see cref="Ms24"/> class with a specified PWM port.
    /// </summary>
    /// <param name="pwm">The PWM port to control the servo.</param>
    public Ms24(IPwmPort pwm)
        : base(pwm,
            new PulseAngle(new Angle(minimumAngle), TimePeriod.FromMilliseconds(minDurationMs)),
            new PulseAngle(new Angle(maximumAngle), TimePeriod.FromMilliseconds(maxDurationMs)))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ms24"/> class with a specified PWM port.
    /// </summary>
    /// <param name="pwmPin">The PWM pin to control the servo.</param>
    public Ms24(IPin pwmPin)
        : base(pwmPin,
            new Frequency(frequencyHz, Frequency.UnitType.Hertz),
            new PulseAngle(new Angle(minimumAngle), TimePeriod.FromMilliseconds(minDurationMs)),
            new PulseAngle(new Angle(maximumAngle), TimePeriod.FromMilliseconds(maxDurationMs)))
    {
    }
}
