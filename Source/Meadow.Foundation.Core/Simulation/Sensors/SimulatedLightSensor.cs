using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Light;
using Meadow.Units;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// Represents a simulated light sensor that implements both ILightSensor and ISimulatedSensor interfaces.
/// </summary>
public class SimulatedLightSensor : BaseSimulatedSensor<Illuminance>, ILightSensor
{
    /// <summary>
    /// Creates a SimulatedLightSensor
    /// </summary>
    /// <param name="initialCondition">The initial illuminance value of the sensor.</param>
    /// <param name="minimumCondition">The minimum illuminance value for the simulation.</param>
    /// <param name="maximumCondition">The maximum illuminance value for the simulation.</param>
    /// <param name="incrementPort">The digital interrupt port used for incrementing the illuminance.</param>
    /// <param name="decrementPort">The digital interrupt port used for decrementing the illuminance.</param>
    public SimulatedLightSensor(Illuminance initialCondition, Illuminance minimumCondition, Illuminance maximumCondition, IDigitalInterruptPort incrementPort, IDigitalInterruptPort decrementPort)
        : base(initialCondition, minimumCondition, maximumCondition, incrementPort, decrementPort)
    {
    }

    /// <summary>
    /// Creates a SimulatedLightSensor
    /// </summary>
    /// <param name="initialCondition">The initial illuminance value of the sensor.</param>
    /// <param name="minimumCondition">The minimum illuminance value for the simulation.</param>
    /// <param name="maximumCondition">The maximum illuminance value for the simulation.</param>
    /// <param name="behavior">The simulation behavior for the sensor (default is SimulationBehavior.RandomWalk).</param>
    public SimulatedLightSensor(Illuminance initialCondition, Illuminance minimumCondition, Illuminance maximumCondition, SimulationBehavior behavior = SimulationBehavior.RandomWalk)
        : base(initialCondition, minimumCondition, maximumCondition, behavior)
    {
    }

    /// <inheritdoc/>
    public Illuminance? Illuminance => CurrentCondition;

    /// <inheritdoc/>
    public override Illuminance ZeroCondition => Units.Illuminance.Zero;

    /// <inheritdoc/>
    protected override double RandomWalkRange => 100d;

    /// <inheritdoc/>
    protected override double SawtoothStepAmount => 10d;

    /// <inheritdoc/>
    protected override Illuminance DecrementCondition(Illuminance currentCondition)
    {
        return new Illuminance(currentCondition.Lux - 100);
    }

    /// <inheritdoc/>
    protected override Illuminance DecrementCondition(Illuminance currentCondition, double conditionDelta)
    {
        return new Illuminance(currentCondition.Lux - conditionDelta);
    }

    /// <inheritdoc/>
    protected override Illuminance IncrementCondition(Illuminance currentCondition)
    {
        return new Illuminance(currentCondition.Lux + 100);
    }

    /// <inheritdoc/>
    protected override Illuminance IncrementCondition(Illuminance currentCondition, double conditionDelta)
    {
        return new Illuminance(currentCondition.Lux + conditionDelta);
    }
}
