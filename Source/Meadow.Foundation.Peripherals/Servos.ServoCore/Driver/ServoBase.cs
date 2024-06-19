using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Servos;

/// <summary>
/// Servo base class
/// </summary>
public abstract class ServoBase : IServo
{
    /// <summary>
    /// Gets the PWM port used to drive the servo
    /// </summary>
    protected IPwmPort PwmPort { get; }

    /// <summary>
    /// Gets the ServoConfig that describes this servo
    /// </summary>
    public ServoConfig Config { get; protected set; }

    /// <summary>
    /// Create a new ServoBase object
    /// </summary>
    /// <param name="pwmPort">PWM port</param>
    /// <param name="config">Servo configuration</param>
    protected ServoBase(IPwmPort pwmPort, ServoConfig config)
    {
        Config = config;

        PwmPort = pwmPort;
        PwmPort.Frequency = config.Frequency;
        PwmPort.DutyCycle = 0;
    }

    /// <summary>
    /// Stop the servo
    /// </summary>
    public virtual void Stop()
    {
        PwmPort.Stop();
    }

    /// <summary>
    /// Note that this calculation expects a pulse duration in microseconds
    /// </summary>
    /// <param name="pulseDuration">Pulse duration</param>
    protected float CalculateDutyCycle(TimeSpan pulseDuration)
    {
        // the pulse duration is dependent on the frequency we're driving the servo at
        return (float)(pulseDuration.TotalSeconds * Config.Frequency.Hertz / 2d);
    }

    /// <inheritdoc/>
    public TimeSpan TrimDuration { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Send a command pulse
    /// </summary>
    /// <param name="pulseDuration">The pulse duration</param>
    protected virtual void SendCommandPulse(TimeSpan pulseDuration)
    {
        var duty = CalculateDutyCycle(pulseDuration + TrimDuration);
        Console.WriteLine($"Duration of: {pulseDuration} is a duty of {duty}");
        PwmPort.DutyCycle = duty;
    }
}