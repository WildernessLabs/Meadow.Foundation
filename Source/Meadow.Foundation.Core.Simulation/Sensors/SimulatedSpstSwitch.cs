using Meadow.Peripherals.Switches;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Switches;

/// <summary>
/// Represents a simulated SPST switch
/// </summary>
public class SimulatedSpstSwitch : ISwitch
{
    /// <inheritdoc/>
    public event EventHandler? Changed;

    private bool state;

    /// <inheritdoc/>
    public bool IsOn => state;

    /// <summary>
    /// Creates a SimulatedSpstSwitch
    /// </summary>
    /// <param name="initialState">The initial state of the switch</param>
    public SimulatedSpstSwitch(bool initialState = false)
    {
        this.state = initialState;
    }

    /// <inheritdoc/>
    public Task<bool> Read()
    {
        return Task.FromResult(state);
    }

    /// <summary>
    /// Sets the state of the simulated switch
    /// </summary>
    /// <param name="newState"></param>
    public void SetState(bool newState)
    {
        if (newState == state) { return; }
        state = newState;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
