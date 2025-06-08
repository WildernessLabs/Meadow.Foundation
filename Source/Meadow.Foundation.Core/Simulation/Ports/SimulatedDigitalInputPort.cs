using Meadow.Hardware;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// A simulated digital input port
/// </summary>
/// <remarks>
/// This class can be useful for testing or simulating digital output 
/// behavior without requiring actual hardware.
/// </remarks>
public class SimulatedDigitalInputPort : IDigitalInputPort
{
    private static int _instanceNumber = 0;

    /// <inheritdoc/>
    public virtual bool State { get; set; }

    /// <summary>
    /// Gets a name or identifier for this simulated port instance.
    /// </summary>
    public string Name { get; }

    /// <inheritdoc/>
    public ResistorMode Resistor { get; set; }
    /// <inheritdoc/>
    public IDigitalChannelInfo Channel { get; }
    /// <inheritdoc/>
    public IPin Pin => throw new System.NotImplementedException();

    /// <summary>
    /// Creates a SimulatedDigitalInputPort
    /// </summary>
    /// <param name="name">An optional port name</param>
    /// <param name="initialState">The port's initial state</param>
    public SimulatedDigitalInputPort(string? name = null, bool initialState = false)
    {
        _instanceNumber++;

        Name = name ?? $"DI{_instanceNumber:D2}";

        State = initialState;
        Channel = new DigitalChannelInfo(
            name: Name,
            inputCapable: true,
            outputCapable: false,
            interruptCapable: false,
            pullDownCapable: false,
            pullUpCapable: false,
            inverseLogic: false,
            interruptGroup: null);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // nop
    }
}
