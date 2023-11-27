using Meadow.Peripherals;
using Meadow.Peripherals.Motors;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Motors.Stepper;

public abstract class GpioStepper : IPositionalMotor
{
    protected double _positionDegrees = 0;
    private double _stepsPerDegree;

    /// <inheritdoc/>
    public RotationDirection Direction { get; protected set; }
    public abstract Angle Position { get; }
    public bool IsMoving { get; protected set; }

    protected abstract Task Rotate(int steps, RotationDirection direction, Frequency rate, CancellationToken cancellationToken = default);

    public abstract int StepsPerRevolution { get; }

    protected GpioStepper()
    {
        _stepsPerDegree = StepsPerRevolution / 360f;
    }

    protected Frequency GetFrequencyForVelocity(AngularVelocity velocity)
    {
        return new Frequency(velocity.DegreesPerSecond * _stepsPerDegree, Frequency.UnitType.Hertz);
    }

    public Task GoTo(Angle position, AngularVelocity velocity, CancellationToken cancellationToken = default)
    {
        RotationDirection shortestDirection;

        // determine shortest path to destination
        double totalDistance;
        if (position.Degrees < _positionDegrees)
        {
            totalDistance = _positionDegrees - position.Degrees;
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
            totalDistance = position.Degrees - _positionDegrees;
            if (totalDistance >= 180)
            {
                totalDistance = totalDistance - 180;
                shortestDirection = RotationDirection.CounterClockwise;
            }
            else
            {
                shortestDirection = RotationDirection.Clockwise;
            }
        }

        return Rotate(new Angle(totalDistance, Angle.UnitType.Degrees), velocity, shortestDirection, cancellationToken);
    }

    public Task GoTo(Angle position, AngularVelocity velocity, RotationDirection direction, CancellationToken cancellationToken = default)
    {
        // convert velocity into frequency based on drive parameters
        var freq = GetFrequencyForVelocity(velocity);

        double totalDistance;

        if (position.Degrees < _positionDegrees)
        {
            totalDistance = direction switch
            {
                RotationDirection.CounterClockwise => _positionDegrees - position.Degrees,
                _ => 360 - _positionDegrees + 360 + position.Degrees
            };
        }
        else
        {
            totalDistance = direction switch
            {
                RotationDirection.Clockwise => position.Degrees - _positionDegrees,
                _ => 360 - _positionDegrees + 360 - position.Degrees
            };
        }

        return Rotate((int)(totalDistance * _stepsPerDegree), direction, freq);
    }

    public Task Rotate(Angle amountToRotate, AngularVelocity velocity, RotationDirection direction, CancellationToken cancellationToken = default)
    {
        // convert velocity into frequency based on drive parameters
        var freq = GetFrequencyForVelocity(velocity);

        return Rotate((int)(amountToRotate.Degrees * _stepsPerDegree), direction, freq, cancellationToken);
    }

    public Task ResetPosition(CancellationToken cancellationToken = default)
    {
        if (IsMoving) throw new Exception("Cannot reset position while the motor is moving.");

        _positionDegrees = 0;

        return Task.CompletedTask;
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
        throw new NotImplementedException();
    }

    public Task RunFor(RotationDirection direction, TimeSpan runTime, AngularVelocity velocity, CancellationToken cancellationToken = default)
    {
        var timeoutTask = Task.Delay(runTime);
        var motorTask = Run(direction, velocity, cancellationToken);

        Task.WaitAny(timeoutTask, motorTask);

        return Task.CompletedTask;
    }

    public Task RunFor(RotationDirection direction, TimeSpan runTime, float power, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Stop(CancellationToken cancellationToken = default)
    {
        if (!IsMoving) return Task.CompletedTask;

        throw new NotImplementedException();
    }
}
