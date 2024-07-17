using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Servos;

/// <summary>
/// Represents an FS90R continuous rotation servo.
/// </summary>
public class Fs90r : ContinuousRotationServo
{
    private const double minDurationMs = 0.9;
    private const double maxDurationMs = 2.1;

    /// <summary>
    /// Initializes a new instance of the <see cref="Fs90r"/> class.
    /// </summary>
    /// <param name="pwm">The PWM port to control the servo.</param>
    /// <exception cref="ArgumentException">Thrown if the PWM frequency is not the required 50 Hz.</exception>
    public Fs90r(IPwmPort pwm)
        : base(pwm, TimePeriod.FromMilliseconds(minDurationMs), TimePeriod.FromMilliseconds(maxDurationMs))
    {
        if (pwm.Frequency.Hertz != RequiredFrequency.Hertz)
        {
            throw new ArgumentException($"PWM Frequency must be {RequiredFrequency.Hertz:N0} Hz");
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Fs90r"/> class.
    /// </summary>
    /// <param name="pwmPin">The PWM pin to control the servo.</param>
    /// <exception cref="ArgumentException">Thrown if the PWM frequency is not the required 50 Hz.</exception>
    public Fs90r(IPin pwmPin)
        : base(pwmPin, RequiredFrequency, TimePeriod.FromMilliseconds(minDurationMs), TimePeriod.FromMilliseconds(maxDurationMs))
    {
    }
}
