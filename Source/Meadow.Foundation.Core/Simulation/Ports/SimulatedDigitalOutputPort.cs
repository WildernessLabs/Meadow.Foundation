using Meadow.Hardware;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// A simulated digital output port that tracks and logs state changes.
/// </summary>
/// <remarks>
/// This class can be useful for testing or simulating digital output 
/// behavior without requiring actual hardware.
/// </remarks>
public class SimulatedDigitalOutputPort : IDigitalOutputPort
{
    /// <summary>
    /// Holds the current output state of the simulated port.
    /// </summary>
    private bool state;

    /// <summary>
    /// Gets the initial state that the port was configured with.
    /// </summary>
    public bool InitialState { get; }

    /// <summary>
    /// Gets a name or identifier for this simulated port instance.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedDigitalOutputPort"/> class.
    /// </summary>
    /// <param name="name">A descriptive name or identifier for the port.</param>
    /// <param name="initialState">
    /// The initial value of the digital port (e.g., <see langword="true" /> for High, 
    /// <see langword="false" /> for Low).
    /// </param>
    public SimulatedDigitalOutputPort(string name, bool initialState)
    {
        Name = name;
        InitialState = state = initialState;
    }

    /// <summary>
    /// Gets or sets the current state of the simulated port.
    /// </summary>
    /// <remarks>
    /// When this property is set, it logs the new state via the 
    /// <see cref="Resolver.Log"/> mechanism. 
    /// </remarks>
    public virtual bool State
    {
        get => state;
        set
        {
            state = value;
            Resolver.Log.Info($"Output {Name} = {State}");
        }
    }

    /// <summary>
    /// Returns <see langword="null"/> since the channel info is not 
    /// meaningful for a simulated port.
    /// </summary>
    public IDigitalChannelInfo Channel => null!;

    /// <summary>
    /// Returns <see langword="null"/> since no physical pin is used.
    /// </summary>
    public IPin Pin => null!;

    /// <summary>
    /// Disposes of the simulated port. 
    /// Currently, this does nothing for the simulation.
    /// </summary>
    public void Dispose()
    {
    }
}
