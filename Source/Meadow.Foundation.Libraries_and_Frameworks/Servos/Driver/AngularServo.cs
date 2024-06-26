using Meadow.Hardware;
using Meadow.Peripherals.Servos;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Servos;

/// <summary>
/// Represents an angular servo
/// </summary>
public partial class AngularServo : ServoBase, IAngularServo
{
    private readonly PulseAngle minPulseAngle;
    private readonly PulseAngle maxPulseAngle;
    private readonly double pulseSecondsPerDegree;
    private readonly double neutralRawPulseWidth;
    private readonly bool portCreated = false;

    /// <inheritdoc/>
    public Angle Angle { get; }
    /// <inheritdoc/>
    public Angle MinimumAngle => minPulseAngle.Angle;
    /// <inheritdoc/>
    public Angle MaximumAngle => maxPulseAngle.Angle;

    /// <summary>
    /// Initializes a new instance of the <see cref="AngularServo"/> class with a specified PWM port and pulse angles.
    /// </summary>
    /// <param name="pwmPort">The PWM port to control the servo.</param>
    /// <param name="minPulseAngle">The pulse angle corresponding to the minimum angle of the servo.</param>
    /// <param name="maxPulseAngle">The pulse angle corresponding to the maximum angle of the servo.</param>
    public AngularServo(IPwmPort pwmPort, PulseAngle minPulseAngle, PulseAngle maxPulseAngle)
        : base(pwmPort)
    {
        this.minPulseAngle = minPulseAngle;
        this.maxPulseAngle = maxPulseAngle;

        var pulseRange = Math.Abs(maxPulseAngle.PulseWidth.Seconds - minPulseAngle.PulseWidth.Seconds);
        var angleRange = Math.Abs(maxPulseAngle.Angle.Degrees - minPulseAngle.Angle.Degrees);

        Resolver.Log.Info($"p range: {pulseRange}  a range: {angleRange}");

        neutralRawPulseWidth = (pulseRange / 2) + minPulseAngle.PulseWidth.Seconds;
        pulseSecondsPerDegree = pulseRange / angleRange;

        Resolver.Log.Info($"neutral: {neutralRawPulseWidth}  s/d: {pulseSecondsPerDegree}");

        Neutral();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AngularServo"/> class with a specified PWM port and pulse angles.
    /// </summary>
    /// <param name="pwmPin">The pin used for PWM control.</param>
    /// <param name="pwmFrequency">The frequency of the PWM signal.</param>
    /// <param name="minPulseAngle">The pulse angle corresponding to the minimum angle of the servo.</param>
    /// <param name="maxPulseAngle">The pulse angle corresponding to the maximum angle of the servo.</param>
    public AngularServo(IPin pwmPin, Frequency pwmFrequency, PulseAngle minPulseAngle, PulseAngle maxPulseAngle)
        : this(pwmPin.CreatePwmPort(pwmFrequency), minPulseAngle, maxPulseAngle)
    {
        portCreated = true;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (portCreated)
        {
            PwmPort?.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    public override void Neutral()
    {
        RotateTo(new Angle((MaximumAngle.Degrees + MinimumAngle.Degrees) / 2d));
    }

    /// <inheritdoc/>
    public void RotateTo(Angle angle)
    {
        if (angle < MinimumAngle || angle > MaximumAngle)
        {
            throw new ArgumentOutOfRangeException(
                nameof(angle),
                $"Angle ({angle.Degrees} must be within servo configuration tolerance ({MinimumAngle.Degrees:n0}-{MaximumAngle.Degrees:n0}");
        }

        var delta = angle.Degrees * pulseSecondsPerDegree;
        var targetPulseWidth = neutralRawPulseWidth + delta;

        Resolver.Log.Info($"delta: {delta}  target: {targetPulseWidth}");

        SetPulseWidthWithTrim(TimePeriod.FromSeconds(targetPulseWidth));
    }
}