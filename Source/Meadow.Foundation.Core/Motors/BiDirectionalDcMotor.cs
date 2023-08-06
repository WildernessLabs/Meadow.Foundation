using Meadow.Hardware;

namespace Meadow.Foundation.Motors;

/// <summary>
/// A generic DC motor whose direction is dictated by two outputs (i.e. a pair of polarity relays)
/// </summary>
public class BiDirectionalDcMotor
{
    private IDigitalOutputPort _outputA;
    private IDigitalOutputPort _outputB;
    private bool _energizeHigh;

    /// <summary>
    /// Gets the current run state of the motor (true == running)
    /// </summary>
    public bool IsRunning => _outputB.State != _outputB.State;

    /// <summary>
    /// Creates an instance of an BiDirectionalDcMotor
    /// </summary>
    /// <param name="outputA">The IDigitalOutputPort driving control relay A</param>
    /// <param name="outputB">The IDigitalOutputPort driving control relay B</param>
    /// <param name="energizeHigh">True if the relay control is positive logic, false if it's inverse logic</param>
    public BiDirectionalDcMotor(
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
    }

    /// <summary>
    /// Start turning the motor clockwise
    /// </summary>
    public void Clockwise()
    {
        _outputA.State = !(_outputB.State = _energizeHigh ? true : false);
    }

    /// <summary>
    /// Start turning the motor counter/anti clockwise
    /// </summary>
    public void CounterClockwise()
    {
        _outputA.State = !(_outputB.State = _energizeHigh ? false : true);
    }

}
