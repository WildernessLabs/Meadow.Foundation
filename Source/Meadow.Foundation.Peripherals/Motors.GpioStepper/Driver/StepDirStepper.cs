using Meadow.Hardware;
using Meadow.Peripherals;
using Meadow.Units;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Motors.Stepper;

/// <summary>
/// A stepper motor that uses a GPIO pulse for step and a GPIO for travel direction
/// </summary>
public class StepDirStepper : GpioStepperBase
{
    private const int MinimumMicrosecondDelayRequiredByDrive = 5; // per the data sheet

    private readonly IDigitalOutputPort _stepPort;
    private readonly IDigitalOutputPort _directionPort;
    private readonly IDigitalOutputPort? _enablePort;
    private float _usPerCall;
    private readonly object _syncRoot = new();
    private int _positionStep;
    private bool _stopRequested = false;
    private bool _isMoving = false;

    /// <summary>
    /// Gets or sets the minimum step dwell time when motor changes from stationary to moving. Motors with more mass or larger steps require more dwell.
    /// </summary>
    public int MinimumStartupDwellMicroseconds { get; set; } = 50;

    /// <summary>
    /// Gets or sets a constant that affects the rate of linear acceleration for the motor. A lower value yields faster acceleration.
    /// Motors with more mass or larger steps require slower acceleration
    /// </summary>
    public int LinearAccelerationConstant { get; set; } = 40;

    /// <inheritdoc/>
    public override int StepsPerRevolution { get; }

    /// <summary>
    /// Gets a value indicating whether or not the logic for the stepper motor driver is inverted.
    /// </summary>
    /// <remarks>
    /// "Normal" is step-on-rising-edge, "Inverse" is step-on-falling-edge of the step port
    /// </remarks>
    public bool InverseLogic { get; }

    /// <inheritdoc/>
    public override bool IsMoving => _isMoving;

    /// <inheritdoc/>
    public override Angle Position => new Angle(_positionStep * (360d / StepsPerRevolution), Angle.UnitType.Degrees);

    /// <inheritdoc/>
    public override AngularVelocity MaxVelocity { get; }

    /// <inheritdoc/>
    public override Task ResetPosition(CancellationToken cancellationToken = default)
    {
        if (IsMoving) throw new Exception("Cannot reset position while the motor is moving.");

        _positionStep = 0;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StepDirStepper"/> class with the specified parameters.
    /// </summary>
    /// <param name="step">The digital output port for controlling the pulse signal.</param>
    /// <param name="direction">The digital output port for controlling the direction of rotation.</param>
    /// <param name="enable">The optional digital output port for enabling or disabling the motor (if available).</param>
    /// <param name="stepsPerRevolution">The number of steps per revolution for the stepper motor (default is 200).</param>
    /// <param name="inverseLogic">A value indicating whether the logic for the stepper motor driver is inverted (default is false).</param>
    public StepDirStepper(
        IDigitalOutputPort step,
        IDigitalOutputPort direction,
        IDigitalOutputPort? enable = null,
        int stepsPerRevolution = 200,
        bool inverseLogic = false
        )
    {
        StepsPerRevolution = stepsPerRevolution;
        MaxVelocity = new AngularVelocity(1 / (StepsPerRevolution * 0.0000010), AngularVelocity.UnitType.RevolutionsPerSecond); // 10us per step minimum speed

        InverseLogic = inverseLogic;

        _stepPort = step;
        _directionPort = direction;
        _enablePort = enable;

        _stepPort.State = !InverseLogic;
        _directionPort.State = InverseLogic;

        if (_enablePort != null)
        {
            _enablePort.State = false;
        }

        CalculateCallDuration();
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    private void CalculateCallDuration()
    {
        // this estimates how long a method call takes on the current platform.
        // this is used below to provide a sub-millisecond "delay" used for step dwell
        var temp = 0;
        var calls = 100000;

        var start = Environment.TickCount;

        for (var i = 0; i < calls; i++)
        {
            temp = i;
        }

        var et = Environment.TickCount - start;

        _usPerCall = et * 1000 / (float)calls;
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    private void MicrosecondSleep(int microseconds)
    {
        var temp = 0;

        for (var i = 0; i < microseconds / _usPerCall; i++)
        {
            temp = i;
        }
    }

    /// <inheritdoc/>
    public override Task Rotate(int steps, RotationDirection direction, Frequency rate, CancellationToken cancellationToken = default)
    {
        if (steps < -1) throw new ArgumentOutOfRangeException(nameof(steps));

        Resolver.Log.Info($"Moving {steps} steps at {rate.Hertz}Hz");

        // usPerCall is calculated async at startup.  This loop is to make sure it's calculated before the first Rotate is run
        while (_usPerCall == 0)
        {
            Thread.Sleep(10);
        }

        if (steps == 0)
        {
            Resolver.Log.Info($"no move required");
            return Task.CompletedTask;
        }

        return Task.Run(() =>
        {
            lock (_syncRoot)
            {
                _isMoving = true;

                try
                {
                    var directionState = direction == RotationDirection.Clockwise;
                    if (InverseLogic) directionState = !directionState;
                    _directionPort.State = directionState;

                    if (_enablePort != null)
                    {
                        _enablePort.State = !InverseLogic;
                    }

                    var targetus = (int)(1000000d / rate.Hertz);

                    if (targetus < MinimumMicrosecondDelayRequiredByDrive) throw new ArgumentOutOfRangeException(
                        "Rate cannot have pulses shorter than 5us. Use 200KHz or less.");

                    var us = targetus < MinimumStartupDwellMicroseconds ? MinimumStartupDwellMicroseconds : targetus;

                    int step;

                    var infiniteRun = steps == -1;
                    if (infiniteRun)
                    {
                        steps = 10;
                    }

                    for (step = 0; step < steps; step++)
                    {
                        if (cancellationToken.IsCancellationRequested || _stopRequested)
                        {
                            break;
                        }

                        _stepPort.State = InverseLogic; // low means "step"

                        MicrosecondSleep(us);

                        _stepPort.State = !InverseLogic;

                        MicrosecondSleep(us);

                        // DEV NOTE:
                        // naive linear acceleration tested only with STP-MTR-34066 motor
                        if (us > targetus && step % LinearAccelerationConstant == 0)
                        {
                            us--;
                        }

                        if (direction == RotationDirection.Clockwise)
                        {
                            _positionStep++;

                            if (_positionStep > StepsPerRevolution)
                            {
                                _positionStep -= StepsPerRevolution;
                            }
                        }
                        else
                        {
                            _positionStep--;

                            if (_positionStep < 0)
                            {
                                _positionStep += StepsPerRevolution;
                            }
                        }

                        if (infiniteRun) step = 0;
                    }

                    Resolver.Log.Info($"done stepping. At position {_positionStep}");

                    if (_enablePort != null)
                    {
                        _enablePort.State = !InverseLogic;
                    }

                    return Task.CompletedTask;
                }
                finally
                {
                    _stopRequested = false;
                    _isMoving = false;
                }
            }
        });
    }

    /// <inheritdoc/>
    public override Task Stop(CancellationToken cancellationToken = default)
    {
        if (IsMoving)
        {
            _stopRequested = true;
        }

        return Task.CompletedTask;
    }

}