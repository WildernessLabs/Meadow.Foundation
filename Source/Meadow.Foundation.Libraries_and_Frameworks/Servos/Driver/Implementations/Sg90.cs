using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Servos;

/// <summary>
/// Represents an SG90 angular servo.
/// </summary>
public class Sg90 : AngularServo
{
    private const double frequencyHz = 50;
    private const double minimumAngle = -90;
    private const double maximumAngle = 90;
    private const double minDurationMs = 0.5;
    private const double maxDurationMs = 2.4;

    /// <summary>
    /// Initializes a new instance of the <see cref="Sg90"/> class with a specified PWM port.
    /// </summary>
    /// <param name="pwm">The PWM port to control the servo.</param>
    public Sg90(IPwmPort pwm)
        : base(pwm,
            new PulseAngle(new Angle(minimumAngle), TimePeriod.FromMilliseconds(minDurationMs)),
            new PulseAngle(new Angle(maximumAngle), TimePeriod.FromMilliseconds(maxDurationMs)))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sg90"/> class with a specified PWM port.
    /// </summary>
    /// <param name="pwmPin">The PWM pin to control the servo.</param>
    public Sg90(IPin pwmPin)
        : base(pwmPin,
            new Frequency(frequencyHz, Frequency.UnitType.Hertz),
            new PulseAngle(new Angle(minimumAngle), TimePeriod.FromMilliseconds(minDurationMs)),
            new PulseAngle(new Angle(maximumAngle), TimePeriod.FromMilliseconds(maxDurationMs)))
    {
    }
}
