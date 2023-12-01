using Meadow.Peripherals;
using Meadow.Peripherals.Motors;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Motors.Stepper;

public abstract class GpioStepper : IPositionalMotor
{
    private double _stepsPerDegree;

    /// <inheritdoc/>
    public RotationDirection Direction { get; protected set; }

    public abstract Angle Position { get; }
    public abstract bool IsMoving { get; }
    protected abstract Task Rotate(int steps, RotationDirection direction, Frequency rate, CancellationToken cancellationToken = default);
    public abstract Task Stop(CancellationToken cancellationToken = default);
    public abstract Task ResetPosition(CancellationToken cancellationToken = default);

    public abstract int StepsPerRevolution { get; }

    protected GpioStepper()
    {
    }

    protected Frequency GetFrequencyForVelocity(AngularVelocity velocity)
    {
        if (StepsPerRevolution <= 0) throw new Exception("StepsPerRevolution must be greater than 0");

        if (_stepsPerDegree == 0)
        {
            _stepsPerDegree = StepsPerRevolution / 360f;
        }

        return new Frequency(velocity.DegreesPerSecond * _stepsPerDegree * 4, Frequency.UnitType.Hertz);
    }

    public Task GoTo(Angle position, AngularVelocity velocity, CancellationToken cancellationToken = default)
    {
        RotationDirection shortestDirection;

        if (Position == position)
        {
            // no move required
            return Task.CompletedTask;
        }

        Resolver.Log.Info($"Currently at: {Position.Degrees} moving to: {position.Degrees}");

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

        Resolver.Log.Info($"Distance to move: {totalDistance} degrees");

        return Rotate(new Angle(totalDistance, Angle.UnitType.Degrees), shortestDirection, velocity, cancellationToken);
    }

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

        Resolver.Log.Info($"Currently at: {start} moving to: {dest}");

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

        Resolver.Log.Info($"Distance to move: {totalDistance} degrees");

        return Rotate((int)(totalDistance * _stepsPerDegree), direction, freq, cancellationToken);
    }

    public Task Rotate(Angle amountToRotate, RotationDirection direction, AngularVelocity velocity, CancellationToken cancellationToken = default)
    {
        // convert velocity into frequency based on drive parameters
        var freq = GetFrequencyForVelocity(velocity);
        var steps = (int)(amountToRotate.Degrees * _stepsPerDegree);

        Resolver.Log.Info($"Rotating {steps} at {freq.Hertz}");

        return Rotate(steps, direction, freq, cancellationToken);
    }

    public Task Run(RotationDirection direction, AngularVelocity velocity, CancellationToken cancellationToken = default)
    {
        // run until cancelled in the specified direction
        if (IsMoving) throw new Exception("Cannot Run while the motor is already moving.");

        var freq = GetFrequencyForVelocity(velocity);
        return Rotate(-1, direction, freq, cancellationToken);
    }

    public Task Run(RotationDirection direction, float power, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("This driver does not support requesting run power");
    }

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

    public virtual Task RunFor(TimeSpan runTime, RotationDirection direction, float power, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("This driver does not support requesting run power");
    }
}
