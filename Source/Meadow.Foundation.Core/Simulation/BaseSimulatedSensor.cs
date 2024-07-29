using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors;

/*
/// <summary>
/// Represents a simulated tone generator that implements both IToneGenerator and ISimulatedSensor interfaces.
/// </summary>
public class SimulatedToneGenerator : IToneGenerator, ISimulatedSensor
{
}

/// <summary>
/// Represents a simulated RgbPwmLed that implements both IRgbPwmLed and ISimulatedSensor interfaces.
/// </summary>
public class SimulatedRgbPwmLed : IRgbPwmLed, ISimulatedSensor
{
}

/// <summary>
/// Represents a simulated barometric pressure sensor that implements both IBarometricPressureSensor and ISimulatedSensor interfaces.
/// </summary>
public class SimulatedBarometricPressureSensor : IBarometricPressureSensor, ISimulatedSensor
{
}

/// <summary>
/// Represents a simulated gas resistance sensor that implements both IGasResistanceSensor and ISimulatedSensor interfaces.
/// </summary>
public class SimulatedGasResistanceSensor : IGasResistanceSensor, ISimulatedSensor
{
}
*/

/// <summary>
/// A base class for simple simulated single-unit sensors
/// </summary>
/// <typeparam name="TUNIT">The type of Unit the device simulates</typeparam>
public abstract class BaseSimulatedSensor<TUNIT> : ISimulatedSensor
    where TUNIT : struct, IComparable
{
    private readonly Random _random = new();
    private TUNIT? _currentCondition;
    private readonly TUNIT? _minCondition;
    private readonly TUNIT? _maxCondition;
    private SimulationBehavior _behavior;
    private Timer? _simulationTimer;
    private int _sawtoothDirection = 1;
    private Timer? _reportTimer;

    /// <summary>
    /// Gets the zero condition for the snsor unit
    /// </summary>
    public abstract TUNIT ZeroCondition { get; }
    /// <summary>
    /// Gets a condition incremented by a step value (used for interrupt value changes)
    /// </summary>
    /// <param name="currentCondition">The initial value to increment </param>
    protected abstract TUNIT IncrementCondition(TUNIT currentCondition);
    /// <summary>
    /// Gets a condition decremented by a step value (used for interrupt value changes)
    /// </summary>
    /// <param name="currentCondition">The initial value to decrement </param>
    protected abstract TUNIT DecrementCondition(TUNIT currentCondition);
    /// <summary>
    /// Gets a condition incremented by the provided root value amount
    /// </summary>
    /// <param name="currentCondition">The initial value to increment</param>
    /// <param name="conditionDelta">The delta to increment by</param>
    protected abstract TUNIT IncrementCondition(TUNIT currentCondition, double conditionDelta);
    /// <summary>
    /// Gets a condition decremented by the provided root value amount
    /// </summary>
    /// <param name="currentCondition">The initial value to decrement</param>
    /// <param name="conditionDelta">The delta to decrement by</param>
    protected abstract TUNIT DecrementCondition(TUNIT currentCondition, double conditionDelta);
    /// <summary>
    /// The upper/lower bound for a random walk
    /// </summary>
    protected abstract double RandomWalkRange { get; }
    /// <summary>
    /// The step amount for a sawtooth simulation
    /// </summary>
    protected abstract double SawtoothStepAmount { get; }

    /// <inheritdoc/>
    public event EventHandler<IChangeResult<TUNIT>> Updated = default!;
    /// <inheritdoc/>
    public SimulationBehavior[] SupportedBehaviors => new SimulationBehavior[] { SimulationBehavior.RandomWalk, SimulationBehavior.Sawtooth };
    /// <inheritdoc/>
    public Type ValueType => typeof(TUNIT);
    /// <inheritdoc/>
    public TimeSpan UpdateInterval { get; private set; }
    /// <inheritdoc/>
    public bool IsSampling { get; private set; }

    /// <summary>
    /// Initializes a new instance of the TemperatureSensorSimulated class.
    /// </summary>
    /// <param name="initialCondition">The initial temperature value of the sensor.</param>
    /// <param name="minimumCondition">The minimum temperature value for the simulation.</param>
    /// <param name="maximumCondition">The maximum temperature value for the simulation.</param>
    /// <param name="incrementPort">The digital interrupt port used for incrementing the temperature.</param>
    /// <param name="decrementPort">The digital interrupt port used for decrementing the temperature.</param>
    public BaseSimulatedSensor(
        TUNIT initialCondition,
        TUNIT minimumCondition,
        TUNIT maximumCondition,
        IDigitalInterruptPort incrementPort,
        IDigitalInterruptPort decrementPort)
    {
        _currentCondition = initialCondition;
        _minCondition = minimumCondition;
        _maxCondition = maximumCondition;

        incrementPort.Changed += (s, e) =>
        {
            var updated = IncrementCondition(CurrentCondition!.Value);
            if (_maxCondition == null || updated.CompareTo(_maxCondition) <= 0)
            {
                CurrentCondition = updated;
            }
        };
        decrementPort.Changed += (s, e) =>
        {
            var updated = DecrementCondition(CurrentCondition!.Value);
            if (updated.CompareTo(_minCondition ?? ZeroCondition) >= 0)
            {
                CurrentCondition = updated;
            }
        };
    }

    /// <summary>
    /// Initializes a new instance of the TemperatureSensorSimulated class with specified parameters.
    /// </summary>
    /// <param name="initialCondition">The initial temperature value of the sensor.</param>
    /// <param name="minimumCondition">The minimum temperature value for the simulation.</param>
    /// <param name="maximumCondition">The maximum temperature value for the simulation.</param>
    /// <param name="behavior">The simulation behavior for the sensor (default is SimulationBehavior.RandomWalk).</param>
    public BaseSimulatedSensor(
        TUNIT initialCondition,
        TUNIT minimumCondition,
        TUNIT maximumCondition,
        SimulationBehavior behavior = SimulationBehavior.RandomWalk)
    {
        _currentCondition = initialCondition;
        _minCondition = minimumCondition;
        _maxCondition = maximumCondition;

        StartSimulation(behavior);
    }

    private void SimulationProc(object? o)
    {
        try
        {
            var delta = _behavior switch
            {
                SimulationBehavior.RandomWalk => _random.NextDouble() * (RandomWalkRange * 2) - RandomWalkRange,
                _ => SawtoothStepAmount * _sawtoothDirection
            };

            if (_currentCondition == null) return;

            var newCondition = IncrementCondition(CurrentCondition!.Value, delta);
            if ((newCondition.CompareTo(_minCondition!.Value) < 0) ||
                (newCondition.CompareTo(_maxCondition!.Value) > 0))
            {
                newCondition = DecrementCondition(CurrentCondition!.Value, delta);

                _sawtoothDirection *= -1;
            }

            CurrentCondition = newCondition;
        }
        finally
        {
            _simulationTimer?.Change(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(-1));
        }
    }

    private void ReportTimerProc(object? o)
    {
        Updated?.Invoke(this, new ChangeResult<TUNIT>(CurrentCondition!.Value, CurrentCondition!.Value));
    }

    /// <inheritdoc/>
    protected TUNIT? CurrentCondition
    {
        get => _currentCondition;
        private set
        {
            if (value.Equals(CurrentCondition)) return;

            if (value != null)
            {
                var previous = _currentCondition;
                _currentCondition = value;
                Updated?.Invoke(this, new ChangeResult<TUNIT>(CurrentCondition!.Value, previous));
            }
        }
    }

    /// <inheritdoc/>
    public Task<TUNIT> Read()
    {
        return Task.FromResult(CurrentCondition ?? ZeroCondition);
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
        if (value is TUNIT condition)
        {
            CurrentCondition = condition;
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
        _simulationTimer = new Timer(SimulationProc, null, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(-1));
    }
}