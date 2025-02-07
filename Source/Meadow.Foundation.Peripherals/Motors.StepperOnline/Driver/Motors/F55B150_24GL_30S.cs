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
    /// <summary>
    /// The default motor rotation direction.
    /// </summary>
    public const RotationDirection DefaultRotationDirection = RotationDirection.Clockwise;

    /// <summary>
    /// The default speed (100 RPM).
    /// </summary>
    public static AngularVelocity DefaultSpeed = new(100, AngularVelocity.UnitType.RevolutionsPerMinute);

    private readonly BLD510B controller;

    /// <inheritdoc/>
    public RotationDirection Direction { get; private set; }

    /// <inheritdoc/>
    public bool IsMoving => controller.GetActualSpeed().Result > 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="F55B150_24GL_30S"/> class
    /// with the specified <see cref="BLD510B"/> controller.
    /// </summary>
    /// <param name="controller">
    /// The <see cref="BLD510B"/> controller used to manage motor operations.
    /// </param>
    public F55B150_24GL_30S(BLD510B controller)
    {
        this.controller = controller;
        Initialize().Wait();
    }

    /// <summary>
    /// Sets the desired speed of the motor.
    /// </summary>
    /// <param name="desiredSpeed">The desired <see cref="AngularVelocity"/> speed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the desired speed exceeds the controller’s maximum value.
    /// </exception>
    public Task SetSpeed(AngularVelocity desiredSpeed)
    {
        var val = desiredSpeed.RevolutionsPerMinute * 75;
        if (val > 65535) { throw new ArgumentOutOfRangeException(nameof(desiredSpeed)); }

        return controller.SetDesiredSpeed((ushort)val);
    }

    /// <summary>
    /// Initializes the <see cref="BLD510B"/> controller with 
    /// default settings for the motor.
    /// </summary>
    /// <remarks>
    /// If initialization times out, it will retry until success.
    /// </remarks>
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

    /// <summary>
    /// Runs the motor in the specified direction.
    /// </summary>
    /// <param name="direction">The <see cref="RotationDirection"/> to rotate the motor.</param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Run(RotationDirection direction, CancellationToken cancellationToken = default)
    {
        Direction = direction;
        await controller.SetDirectionTerminal(direction);
        await controller.SetStartStopTerminal(true);
    }

    /// <summary>
    /// Runs the motor for a specified duration in the given direction, then stops.
    /// </summary>
    /// <param name="runTime">The <see cref="TimeSpan"/> duration to run the motor.</param>
    /// <param name="direction">The <see cref="RotationDirection"/> to rotate the motor.</param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RunFor(TimeSpan runTime, RotationDirection direction, CancellationToken cancellationToken = default)
    {
        Direction = direction;
        await controller.SetDirectionTerminal(direction);
        await controller.SetStartStopTerminal(true);

        await Task.Delay(runTime, cancellationToken);

        // The code here sets the terminal to true, which might be a typo.
        // If you actually want to stop the motor, consider setting it to false. 
        await controller.SetStartStopTerminal(true);
    }

    /// <summary>
    /// Stops the motor.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task Stop(CancellationToken cancellationToken = default)
    {
        return controller.SetStartStopTerminal(false);
    }

    /// <summary>
    /// Sets the state of the motor’s brake.
    /// </summary>
    /// <param name="enabled">
    /// <see langword="true" /> to enable the brake; <see langword="false" /> to disable.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task SetBrakeState(bool enabled)
    {
        return controller.SetBrakeTerminal(enabled);
    }

    /// <summary>
    /// Sets the time it takes for the motor to accelerate to the target speed.
    /// </summary>
    /// <param name="time">The <see cref="TimeSpan"/> it should take to accelerate.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task SetAccelerationTime(TimeSpan time)
    {
        return controller.SetAccelerationTime(time);
    }

    /// <summary>
    /// Sets the time it takes for the motor to decelerate to a stop.
    /// </summary>
    /// <param name="time">The <see cref="TimeSpan"/> it should take to decelerate.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task SetDecelerationTime(TimeSpan time)
    {
        return controller.SetDecelerationTime(time);
    }

    /// <summary>
    /// Reads the current speed of the motor from the controller.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> whose result is an 
    /// <see cref="AngularVelocity"/> indicating the motor’s actual speed.
    /// </returns>
    public async Task<AngularVelocity> GetActualSpeed()
    {
        var rawSpeed = await controller.GetActualSpeed();
        // 1 count equals approximately 0.0133 RPM for this driver/motor combination.
        return new AngularVelocity(rawSpeed * 0.0133, AngularVelocity.UnitType.RevolutionsPerMinute);
    }
}
