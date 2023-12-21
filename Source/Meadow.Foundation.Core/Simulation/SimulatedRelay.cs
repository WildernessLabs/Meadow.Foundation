using Meadow.Peripherals.Relays;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading;

namespace Meadow.Foundation.Simulation;

/// <summary>
/// Represents a simulated relay that implements both IRelay and ISimulatedSensor interfaces.
/// </summary>
public class SimulatedRelay : IRelay, ISimulatedSensor
{
    private RelayState _state;
    private Timer? _simulationTimer;

    /// <inheritdoc/>
    public event EventHandler<RelayState> OnChanged = default!;

    /// <inheritdoc/>
    public RelayType Type => RelayType.NormallyOpen;

    /// <summary>
    /// Gets the name of the Relay
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the SimulatedRelay class with a specified name.
    /// </summary>
    /// <param name="name">The name of the simulated relay.</param>
    public SimulatedRelay(string name)
    {
        Name = name;
    }

    /// <inheritdoc/>
    public RelayState State
    {
        get => _state;
        set
        {
            if (value == _state) return;
            _state = value;
            OnChanged?.Invoke(this, State);
        }
    }

    /// <inheritdoc/>
    public SimulationBehavior[] SupportedBehaviors => new SimulationBehavior[] { SimulationBehavior.Sawtooth };
    /// <inheritdoc/>
    public Type ValueType => typeof(bool);

    /// <inheritdoc/>
    public void Toggle()
    {
        State = State switch
        {
            RelayState.Open => RelayState.Closed,
            _ => RelayState.Open,
        };
    }

    /// <inheritdoc/>
    public void SetSensorValue(object value)
    {
        if (value is bool b)
        {
            State = b switch
            {
                true => RelayState.Closed,
                _ => RelayState.Open
            };
        }
        else if (value is RelayState s)
        {
            State = s;
        }
        else
        {
            throw new ArgumentException($"Expected a parameter of type '{ValueType.Name}' but received a '{value.GetType().Name}'");
        }
    }

    /// <inheritdoc/>
    public void StartSimulation(SimulationBehavior behavior)
    {
        if (_simulationTimer == null)
        {
            _simulationTimer = new Timer((o) =>
            {
                Toggle();
            },
            null,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(5));
        }
    }
}
