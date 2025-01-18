using Meadow.Foundation.MotorControllers.StepperOnline;
using Meadow.Peripherals;
using Meadow.Peripherals.Motors;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Motors.StepperOnline;

/// <summary>
/// A 24V, 100RPM, 30:1 gear-reduction BLDC motor
/// </summary>
public class F55B150_24GL_30S : IMotor
{
    private BLD510B controller;

    /// <summary>
    /// The default motor rotation direction
    /// </summary>
    public const RotationDirection DefaultRotationDirection = RotationDirection.Clockwise;
    public static AngularVelocity DefaultSpeed = new AngularVelocity(100, AngularVelocity.UnitType.RevolutionsPerMinute);

    /// <inheritdoc/>
    public RotationDirection Direction { get; private set; }

    /// <inheritdoc/>
    public bool IsMoving => controller.GetActualSpeed().Result > 0;

    public F55B150_24GL_30S(BLD510B controller)
    {
        this.controller = controller;

        Initialize().Wait();
    }

    public Task SetSpeed(AngularVelocity desiredSpeed)
    {
        var val = desiredSpeed.RevolutionsPerMinute * 75;
        if (val > 65535) throw new ArgumentOutOfRangeException(nameof(desiredSpeed));
        return controller.SetDesiredSpeed((ushort)val);
    }

    private async Task Initialize()
    {
        var succeeded = false;

        while (!succeeded)
        {
            try
            {
                await controller.SetStartStopTerminal(false);
                await controller.SetNumberOfMotorPolePairs(10);
                await controller.SetSpeedControl(SpeedControl.RS485);
                Direction = DefaultRotationDirection;
                await controller.SetDirectionTerminal(Direction);
                await SetSpeed(DefaultSpeed);
                succeeded = true;
            }
            catch (TimeoutException)
            {
                Resolver.Log.Warn("Timeout initializing");
                await Task.Delay(500);
            }
        }
    }

    public async Task Run(RotationDirection direction, CancellationToken cancellationToken = default)
    {
        Direction = direction;
        await controller.SetDirectionTerminal(direction);
        await controller.SetStartStopTerminal(true);
    }

    public async Task RunFor(TimeSpan runTime, RotationDirection direction, CancellationToken cancellationToken = default)
    {
        Direction = direction;
        await controller.SetDirectionTerminal(direction);
        await controller.SetStartStopTerminal(true);
        await Task.Delay(runTime, cancellationToken);
        await controller.SetStartStopTerminal(true);
    }

    public Task Stop(CancellationToken cancellationToken = default)
    {
        return controller.SetStartStopTerminal(false);
    }

    public Task SetBrakeState(bool enabled)
    {
        return controller.SetBrakeTerminal(enabled);
    }

    public Task SetAccelerationTime(TimeSpan time)
    {
        return controller.SetAccelerationTime(time);
    }

    public Task SetDecelerationTime(TimeSpan time)
    {
        return controller.SetDecelerationTime(time);
    }

    public async Task<AngularVelocity> GetActualSpeed()
    {
        var rawSpeed = await controller.GetActualSpeed();
        return new AngularVelocity(rawSpeed * 0.0133, AngularVelocity.UnitType.RevolutionsPerMinute);
    }
}
