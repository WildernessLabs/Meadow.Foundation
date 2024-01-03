using Meadow.Peripherals;
using Meadow.Peripherals.Motors;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Motors.Stepper;

/// <summary>
/// Represents an abstract GPIO-based stepper motor.
/// </summary>
public abstract class GpioStepperBase : IStepperMotor
{
    private double _stepsPerDegree;

    /// <inheritdoc/>
    public RotationDirection Direction { get; protected set; }

    /// <inheritdoc/>
    public abstract Angle Position { get; }

    /// <inheritdoc/>
    public abstract bool IsMoving { get; }

    /// <inheritdoc/>
    public abstract Task Rotate(int steps, RotationDirection direction, Frequency rate, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task Stop(CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task ResetPosition(CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract AngularVelocity MaxVelocity { get; }

    /// <inheritdoc/>
    public abstract int StepsPerRevolution { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GpioStepperBase"/> class.
    /// </summary>
    protected GpioStepperBase()
    {
    }

    /// <summary>
    /// Gets the frequency corresponding to the specified angular velocity.
    /// </summary>
    /// <param name="velocity">The angular velocity.</param>
    /// <returns>The frequency required for the specified angular velocity.</returns>
    protected Frequency GetFrequencyForVelocity(AngularVelocity velocity)
    {
        if (StepsPerRevolution <= 0) throw new Exception("StepsPerRevolution must be greater than 0");

        if (_stepsPerDegree == 0)
        {
            _stepsPerDegree = StepsPerRevolution / 360f;
        }

        return new Frequency(velocity.DegreesPerSecond * _stepsPerDegree * 4, Frequency.UnitType.Hertz);
    }

    /// <inheritdoc/>
    public Task GoTo(Angle position, AngularVelocity velocity, CancellationToken cancellationToken = default)
    {
        RotationDirection shortestDirection;

        if (Position == position)
        {
            // no move required
            return Task.CompletedTask;
        }

        // determine shortest path to destination
        double totalDistance;
        if (position.Degrees < Position.Degrees)
        {
            totalDistance = Position.Degrees - position.Degrees;
            if (totalDistance < 180)
            {
                shortestDirection = RotationDirection.CounterClockwise;
            }
            else
            {
                totalDistance = totalDistance - 180;
                shortestDirection = RotationDirection.Clockwise;
            }
        }
        else
        {
            totalDistance = position.Degrees - Position.Degrees;
            if (totalDistance > 180)
            {
                totalDistance = totalDistance - 180;
                shortestDirection = RotationDirection.CounterClockwise;
            }
            else
            {
                shortestDirection = RotationDirection.Clockwise;
            }
        }

        return Rotate(new Angle(totalDistance, Angle.UnitType.Degrees), shortestDirection, velocity, cancellationToken);
    }

    /// <inheritdoc/>
    public Task GoTo(Angle position, RotationDirection direction, AngularVelocity velocity, CancellationToken cancellationToken = default)
    {
        if (Position == position)
        {
            // no move required
            return Task.CompletedTask;
        }

        var dest = position.Degrees;
        var start = Position.Degrees;

        while (dest < 0) dest += 360;
        dest %= 360;

        // convert velocity into frequency based on drive parameters
        var freq = GetFrequencyForVelocity(velocity);

        double totalDistance;

        if (dest < start)
        {
            totalDistance = direction switch
            {
                RotationDirection.CounterClockwise => start - dest,
                _ => 360 - start + dest
            };
        }
        else
        {
            totalDistance = direction switch
            {
                RotationDirection.Clockwise => dest - start,
                _ => start + 360 - dest
            };
        }

        return Rotate((int)(totalDistance * _stepsPerDegree), direction, freq, cancellationToken);
    }

    /// <inheritdoc/>
    public Task Rotate(Angle amountToRotate, RotationDirection direction, AngularVelocity velocity, CancellationToken cancellationToken = default)
    {
        // convert velocity into frequency based on drive parameters
        var freq = GetFrequencyForVelocity(velocity);
        var steps = (int)(amountToRotate.Degrees * _stepsPerDegree);

        return Rotate(steps, direction, freq, cancellationToken);
    }

    /// <inheritdoc/>
    public Task Run(RotationDirection direction, AngularVelocity velocity, CancellationToken cancellationToken = default)
    {
        // run until cancelled in the specified direction
        if (IsMoving) throw new Exception("Cannot Run while the motor is already moving.");

        var freq = GetFrequencyForVelocity(velocity);
        return Rotate(-1, direction, freq, cancellationToken);
    }

    /// <inheritdoc/>
    public Task RunFor(TimeSpan runTime, RotationDirection direction, AngularVelocity velocity, CancellationToken cancellationToken = default)
    {
        var timeoutTask = Task.Delay(runTime);
        var motorTask = Run(direction, velocity, cancellationToken);
        var t = Task.WaitAny(timeoutTask, motorTask);

        if (t == 0)
        {
            // tell the motor to stop
            Stop();

            // wait for the motor to finish
            motorTask.Wait();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task Run(RotationDirection direction, float speed, CancellationToken cancellationToken = default)
    {
        if (speed < 0 || speed > 100) throw new ArgumentOutOfRangeException(nameof(speed));

        var velocity = new AngularVelocity(MaxVelocity.RevolutionsPerSecond * (speed / 100));

        return Run(direction, velocity, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task RunFor(TimeSpan runTime, RotationDirection direction, float speed, CancellationToken cancellationToken = default)
    {
        if (speed < 0 || speed > 100) throw new ArgumentOutOfRangeException(nameof(speed));

        var velocity = new AngularVelocity(MaxVelocity.RevolutionsPerSecond * (speed / 100));

        return RunFor(runTime, direction, velocity, cancellationToken);
    }

    /// <inheritdoc/>
    public Task RunFor(TimeSpan runTime, RotationDirection direction, CancellationToken cancellationToken = default)
    {
        return RunFor(runTime, direction, 100f, cancellationToken);
    }

    /// <inheritdoc/>
    public Task Run(RotationDirection direction, CancellationToken cancellationToken = default)
    {
        return Run(direction, 100f, cancellationToken);
    }
}
