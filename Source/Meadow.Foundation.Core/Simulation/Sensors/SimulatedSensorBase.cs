using Meadow.Peripherals.Sensors;
using System;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// Contains the base logic form simple simulated sensors
/// </summary>
public abstract class SimulatedSensorBase : ISimulatedSensor
{
    private Random random = new Random();

    /// <summary>
    /// The currently set behavior for the sensor
    /// </summary>
    protected SimulationBehavior SimulationBehavior { get; private set; }

    /// <inheritdoc/>
    public virtual SimulationBehavior[] SupportedBehaviors => new[] { SimulationBehavior.None };

    /// <inheritdoc/>
    public abstract Type ValueType { get; }

    /// <inheritdoc/>
    public abstract void SetSensorValue(object value);

    /// <inheritdoc/>
    public virtual void StartSimulation(SimulationBehavior behavior)
    {
        SimulationBehavior = behavior;
    }

    /// <summary>
    /// Generates a random double value between the two provided values
    /// </summary>
    /// <param name="minValue">Inclusive lower bound value</param>
    /// <param name="maxValue">Exclusive upper bound value</param>
    /// <returns></returns>
    protected double GetRandomDouble(double minValue, double maxValue)
    {
        return random.NextDouble() * (maxValue - minValue) + minValue;
    }
}
