using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Distance;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// Represents a simulated range finder.
/// </summary>
public class SimulatedRangeFinder : SimulatedSamplingSensorBase<Length>, IRangeFinder
{
    private readonly Length? minimumLength;
    private readonly Length? maximumLength;

    /// <inheritdoc/>
    public override Type ValueType => typeof(Length);

    /// <inheritdoc/>
    public Length? Distance { get; protected set; }

    /// <summary>
    /// Creates a Simulated Range Finder
    /// </summary>
    /// <param name="initialLength"></param>
    /// <param name="minimumLength"></param>
    /// <param name="maximumLength"></param>
    /// <param name="simulationBehavior"></param>
    public SimulatedRangeFinder(
        Length initialLength,
        Length minimumLength,
        Length maximumLength,
        SimulationBehavior simulationBehavior = SimulationBehavior.None)
    {
        Distance = initialLength;

        this.minimumLength = minimumLength;
        this.maximumLength = maximumLength;

        StartSimulation(simulationBehavior);
    }

    /// <inheritdoc/>
    public override void SetSensorValue(object value)
    {
        Distance = (Length)value;
    }

    /// <inheritdoc/>
    protected override Length GenerateSimulatedValue(SimulationBehavior behavior)
    {
        switch (behavior)
        {
            case SimulationBehavior.RandomWalk:
                Distance = new Length(GetRandomDouble(minimumLength!.Value.Centimeters, maximumLength!.Value.Centimeters), Length.UnitType.Centimeters);
                break;
            default:
                Distance = new Length((minimumLength!.Value.Centimeters + maximumLength!.Value.Centimeters) / 2, Length.UnitType.Centimeters);
                break;
        }

        return Distance!.Value;
    }

    /// <inheritdoc/>
    public void MeasureDistance()
    {
        Distance = GenerateSimulatedValue(SimulationBehavior.RandomWalk);
    }
}