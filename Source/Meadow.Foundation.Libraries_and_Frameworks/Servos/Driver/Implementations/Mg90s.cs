using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Servos;

/// <summary>
/// Represents an MG90S angular servo.
/// </summary>
public class Mg90s : AngularServo
{
    private const double frequencyHz = 50;
    private const double minimumAngle = -90;
    private const double maximumAngle = 90;
    private const double minDurationMs = 0.6;
    private const double maxDurationMs = 2.4;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mg90s"/> class with a specified PWM port.
    /// </summary>
    /// <param name="pwm">The PWM port to control the servo.</param>
    public Mg90s(IPwmPort pwm)
        : base(pwm,
            new PulseAngle(new Angle(minimumAngle), TimePeriod.FromMilliseconds(minDurationMs)),
            new PulseAngle(new Angle(maximumAngle), TimePeriod.FromMilliseconds(maxDurationMs)))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mg90s"/> class with a specified PWM port.
    /// </summary>
    /// <param name="pwmPin">The PWM pin to control the servo.</param>
    public Mg90s(IPin pwmPin)
        : base(pwmPin,
            new Frequency(frequencyHz, Frequency.UnitType.Hertz),
            new PulseAngle(new Angle(minimumAngle), TimePeriod.FromMilliseconds(minDurationMs)),
            new PulseAngle(new Angle(maximumAngle), TimePeriod.FromMilliseconds(maxDurationMs)))
    {
    }
}
