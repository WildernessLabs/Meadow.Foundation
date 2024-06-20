using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Servos;

/// <summary>
/// Represents an FS90R continuous rotation servo.
/// </summary>
public class Fs90r : ContinuousRotationServo
{
    private static Frequency? requiredFrequency;

    /// <summary>
    /// Gets the required PWM frequency for the FS90R servo.
    /// </summary>
    public static Frequency RequiredFrequency => requiredFrequency ??= new Frequency(50, Frequency.UnitType.Hertz);

    /// <summary>
    /// Initializes a new instance of the <see cref="Fs90r"/> class.
    /// </summary>
    /// <param name="pwm">The PWM port to control the servo.</param>
    /// <exception cref="ArgumentException">Thrown if the PWM frequency is not the required 50 Hz.</exception>
    public Fs90r(IPwmPort pwm)
        : base(pwm, TimeSpan.FromMilliseconds(0.9d), TimeSpan.FromMilliseconds(2.1d))
    {
        if (pwm.Frequency.Hertz != RequiredFrequency.Hertz)
        {
            throw new ArgumentException($"PWM Frequency must be {RequiredFrequency.Hertz:N0} Hz");
        }
    }
}
