using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// Represents a simulated temperature sensor that implements both ITemperatureSensor and ISimulatedSensor interfaces.
/// </summary>
public class SimulatedTemperatureSensor : ITemperatureSensor, ISimulatedSensor
{
    private readonly Random _random = new();
    private Units.Temperature? _temperature;
    private readonly Units.Temperature? _minTemperature;
    private readonly Units.Temperature? _maxTemperature;
    private SimulationBehavior _behavior;
    private int _sawtoothDirection = 1;
    private Timer? _reportTimer;
    private Timer? _simulationTimer;

    /// <inheritdoc/>
    public event EventHandler<IChangeResult<Units.Temperature>> Updated = default!;
    /// <inheritdoc/>
    public SimulationBehavior[] SupportedBehaviors => new SimulationBehavior[] { SimulationBehavior.RandomWalk, SimulationBehavior.Sawtooth };
    /// <inheritdoc/>
    public Type ValueType => typeof(Units.Temperature);
    /// <inheritdoc/>
    public TimeSpan UpdateInterval { get; private set; }
    /// <inheritdoc/>
    public bool IsSampling { get; private set; }

    /// <summary>
    /// Initializes a new instance of the TemperatureSensorSimulated class.
    /// </summary>
    /// <param name="initialTemperature">The initial temperature value of the sensor.</param>
    /// <param name="incrementPort">The digital interrupt port used for incrementing the temperature.</param>
    /// <param name="decrementPort">The digital interrupt port used for decrementing the temperature.</param>
    public SimulatedTemperatureSensor(
        Units.Temperature initialTemperature,
        IDigitalInterruptPort incrementPort,
        IDigitalInterruptPort decrementPort)
    {
        _temperature = initialTemperature;

        incrementPort.Changed += (s, e) =>
        {
            Temperature = new Units.Temperature(Temperature!.Value.Fahrenheit + 0.5, Meadow.Units.Temperature.UnitType.Fahrenheit);
        };
        decrementPort.Changed += (s, e) =>
        {
            Temperature = new Units.Temperature(Temperature!.Value.Fahrenheit - 0.5, Meadow.Units.Temperature.UnitType.Fahrenheit);
        };
    }

    /// <summary>
    /// Initializes a new instance of the TemperatureSensorSimulated class with specified parameters.
    /// </summary>
    /// <param name="initialTemperature">The initial temperature value of the sensor.</param>
    /// <param name="minimumTemperature">The minimum temperature value for the simulation.</param>
    /// <param name="maximumTemperature">The maximum temperature value for the simulation.</param>
    /// <param name="behavior">The simulation behavior for the sensor (default is SimulationBehavior.RandomWalk).</param>
    public SimulatedTemperatureSensor(
        Units.Temperature initialTemperature,
        Units.Temperature minimumTemperature,
        Units.Temperature maximumTemperature,
        SimulationBehavior behavior = SimulationBehavior.RandomWalk)
    {
        _temperature = initialTemperature;
        _minTemperature = minimumTemperature;
        _maxTemperature = maximumTemperature;

        StartSimulation(behavior);
    }

    private void SimulationProc(object? o)
    {
        var delta = _behavior switch
        {
            SimulationBehavior.RandomWalk => _random.Next(-10, 10) / 10d,
            _ => 0.1 * _sawtoothDirection
        };

        if (_temperature == null) return;

        var newTemp = _temperature.Value.Celsius + delta;
        if ((newTemp < _minTemperature!.Value.Celsius) ||
            (newTemp > _maxTemperature!.Value.Celsius))
        {
            newTemp = _temperature.Value.Celsius - delta;
            _sawtoothDirection *= -1;
        }

        Temperature = new Units.Temperature(newTemp, Meadow.Units.Temperature.UnitType.Celsius);
    }

    private void ReportTimerProc(object? o)
    {
        Updated?.Invoke(this, new ChangeResult<Units.Temperature>(this.Temperature!.Value, this.Temperature!.Value));
    }

    /// <inheritdoc/>
    public Units.Temperature? Temperature
    {
        get => _temperature;
        private set
        {
            if (value == Temperature) return;

            if (value != null)
            {
                var previous = _temperature;
                _temperature = value;
                Updated?.Invoke(this, new ChangeResult<Units.Temperature>(Temperature!.Value, previous));
            }
        }
    }

    /// <inheritdoc/>
    public Task<Units.Temperature> Read()
    {
        return Task.FromResult(Temperature ?? Units.Temperature.AbsoluteZero);
    }

    /// <summary>
    /// Starts updating the sensor value at the specified interval
    /// </summary>
    /// <param name="updateInterval"></param>
    public void StartUpdating(TimeSpan? updateInterval = null)
    {
        UpdateInterval = updateInterval ?? TimeSpan.FromSeconds(1);
        IsSampling = true;
        _reportTimer = new Timer(ReportTimerProc, null, updateInterval!.Value, updateInterval.Value);
    }

    /// <summary>
    /// Stops updating the sensor
    /// </summary>
    public void StopUpdating()
    {
        IsSampling = false;
        _reportTimer?.Dispose();
    }

    /// <inheritdoc/>
    public void SetSensorValue(object value)
    {
        if (value is Units.Temperature temperature)
        {
            Temperature = temperature;
        }
        else
        {
            throw new ArgumentException($"Expected a parameter of type '{ValueType.Name}' but received a '{value.GetType().Name}'");
        }
    }

    /// <inheritdoc/>
    public void StartSimulation(SimulationBehavior behavior)
    {
        _behavior = behavior;
        _simulationTimer = new Timer(SimulationProc, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }
}
