using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.CurrentLoop;

/// <summary>
/// Implementation of a simulated current loop generator
/// </summary>
public class SimulatedCurrentLoopGenerator : ICurrentLoopGenerator
{
    private Current _lastCurrent;

    /// <summary>
    /// Initializes a new instance of a SimulatedCurrentLoopTransmitter
    /// </summary>
    public SimulatedCurrentLoopGenerator()
    {
        _lastCurrent = 0.004.Amps();
    }

    /// <summary>
    /// Initializes a new instance of a SimulatedCurrentLoopTransmitter
    /// </summary>
    /// <param name="startCurrent">An intial value for the output current</param>
    public SimulatedCurrentLoopGenerator(Current startCurrent)
    {
        _lastCurrent = startCurrent;
    }

    /// <inheritdoc/>
    public Current GetOutputCurrent()
    {
        return _lastCurrent;
    }

    /// <inheritdoc/>
    public Task SetOutputCurrent(Current current)
    {
        _lastCurrent = current;
        return Task.CompletedTask;
    }
}
