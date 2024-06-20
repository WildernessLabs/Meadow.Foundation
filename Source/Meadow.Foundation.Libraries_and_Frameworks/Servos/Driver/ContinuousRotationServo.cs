using Meadow.Hardware;
using Meadow.Peripherals;
using Meadow.Peripherals.Servos;
using System;

namespace Meadow.Foundation.Servos;

/// <summary>
/// Represents a continuous rotation servo, extending the base servo functionality.
/// </summary>
public class ContinuousRotationServo : ServoBase, IContinuousRotationServo
{
    private readonly TimeSpan maximumPulseDuration;
    private readonly TimeSpan rawNeutralPulseDuration;

    /// <inheritdoc/>
    public double Speed { get; private set; }
    /// <inheritdoc/>
    public RotationDirection Direction { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContinuousRotationServo"/> class with specified PWM port and pulse durations.
    /// </summary>
    /// <param name="pwm">The PWM port to control the servo.</param>
    /// <param name="minimumPulseDuration">The minimum pulse duration for the servo.</param>
    /// <param name="maximumPulseDuration">The maximum pulse duration for the servo.</param>
    public ContinuousRotationServo(IPwmPort pwm, TimeSpan minimumPulseDuration, TimeSpan maximumPulseDuration)
        : base(pwm)
    {
        this.maximumPulseDuration = maximumPulseDuration;
        rawNeutralPulseDuration = TimeSpan.FromSeconds(
            (maximumPulseDuration.TotalSeconds - minimumPulseDuration.TotalSeconds) / 2
            + minimumPulseDuration.TotalSeconds);

        Neutral();
    }

    /// <inheritdoc/>
    public void Rotate(RotationDirection direction, double speed)
    {
        if (speed is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(speed), "speed must be 0.0 - 1.0.");
        }

        var pulseDuration = CalculatePulseDuration(direction, speed);

        SetPulseWidthWithTrim(pulseDuration);

        Speed = speed;
        Direction = direction;
    }

    /// <summary>
    /// Continuous rotation servos usually have a zero speed at their midpoint pulse 
    /// duration (between min and max). As you lower the duration from midpoint, they 
    /// rotate clockwise and rotate their fastest at the minimum pulse duration. As 
    /// you increase the pulse duration, they rotate counter-clockwise.
    /// </summary>
    private TimeSpan CalculatePulseDuration(RotationDirection direction, double speed)
    {
        if (speed is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(speed), "Speed must be between 0 and 1, inclusive");
        }

        if (speed == 0)
        {
            return rawNeutralPulseDuration;
        }

        // distance from the raw neutral - proportial to speed
        var delta = (maximumPulseDuration.TotalSeconds - rawNeutralPulseDuration.TotalSeconds) * speed;
        delta *= direction == RotationDirection.Clockwise ? -1 : 1;
        var calculatedDuration = rawNeutralPulseDuration.Add(TimeSpan.FromSeconds(delta));

        return calculatedDuration;
    }

    /// <inheritdoc/>
    public override void Disable()
    {
        Speed = 0;
        base.Disable();
    }

    /// <inheritdoc/>
    public override void Neutral()
    {
        Rotate(RotationDirection.Clockwise, 0);
    }
}
