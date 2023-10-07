using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Motors;

/// <summary>
/// A generic DC motor whose direction is dictated by two outputs (i.e. a pair of polarity relays)
/// </summary>
public class BidirectionalDcMotor
{
    public enum MotorState
    {
        Stopped,
        RunningClockwise,
        RunningCounterclockwise
    }

    public event EventHandler<MotorState> StateChanged = default!;

    private readonly IDigitalOutputPort _outputA;
    private readonly IDigitalOutputPort _outputB;
    private readonly bool _energizeHigh;

    /// <summary>
    /// Gets the current run state of the motor
    /// </summary>
    public MotorState State
    {
        get
        {
            var a = _energizeHigh ? _outputA.State : !_outputA.State;
            var b = _energizeHigh ? _outputB.State : !_outputB.State;

            if (a == b)
            {
                return MotorState.Stopped;
            }
            if (a && !b)
            {
                return MotorState.RunningClockwise;
            }
            return MotorState.RunningCounterclockwise;
        }
    }

    /// <summary>
    /// Creates an instance of an BiDirectionalDcMotor
    /// </summary>
    /// <param name="outputA">The IDigitalOutputPort driving control relay A</param>
    /// <param name="outputB">The IDigitalOutputPort driving control relay B</param>
    /// <param name="energizeHigh">True if the relay control is positive logic, false if it's inverse logic</param>
    public BidirectionalDcMotor(
        IDigitalOutputPort outputA,
        IDigitalOutputPort outputB,
        bool energizeHigh = true)
    {
        _outputA = outputA;
        _outputB = outputB;
        _energizeHigh = energizeHigh;
    }

    /// <summary>
    /// Stop turning the motor
    /// </summary>
    public void Stop()
    {
        _outputA.State = _outputB.State = _energizeHigh ? false : true;
        StateChanged?.Invoke(this, State);
    }

    /// <summary>
    /// Start turning the motor clockwise
    /// </summary>
    public void StartClockwise()
    {
        _outputA.State = !(_outputB.State = _energizeHigh ? true : false);
        StateChanged?.Invoke(this, State);
    }

    /// <summary>
    /// Start turning the motor counter/anti clockwise
    /// </summary>
    public void StartCounterClockwise()
    {
        _outputA.State = !(_outputB.State = _energizeHigh ? false : true);
        StateChanged?.Invoke(this, State);
    }
}
