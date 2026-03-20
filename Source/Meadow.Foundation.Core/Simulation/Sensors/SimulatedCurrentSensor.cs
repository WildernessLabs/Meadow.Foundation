using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// Represents a simulated current sensor
/// </summary>
public class SimulatedCurrentSensor : SimulatedSamplingSensorBase<Current>, ISamplingCurrentSensor
{
    private readonly Current maxCurrent;
    private readonly Current minCurrent;

    /// <inheritdoc/>
    public Current? Current { get; private set; }

    /// <inheritdoc/>
    public override SimulationBehavior[] SupportedBehaviors => new[] { SimulationBehavior.RandomWalk };

    /// <inheritdoc/>
    public override Type ValueType => typeof(Current);

    /// <summary>
    /// Creates a SimulatedCurrentSensor instance
    /// </summary>
    public SimulatedCurrentSensor(Current? maxCurrent = null, Current? minCurrent = null)
    {
        this.minCurrent = minCurrent ?? new Current(0, Units.Current.UnitType.Amps);
        this.maxCurrent = maxCurrent ?? new Current(1, Units.Current.UnitType.Amps);

        Current = 0.Amps();
    }

    /// <inheritdoc/>
    public override void SetSensorValue(object value)
    {
        Current = (Current)value;
    }

    /// <inheritdoc/>
    protected override Current GenerateSimulatedValue(SimulationBehavior behavior)
    {
        switch (behavior)
        {
            case SimulationBehavior.RandomWalk:
                var r = GetRandomDouble(minCurrent.Amps, maxCurrent.Amps);
                Current = new Current(r);
                break;
        }

        return Current!.Value;
    }

    /// <inheritdoc/>
    public ValueTask<Current> ReadCurrent()
    {
        return new ValueTask<Current>(Current ?? Units.Current.Zero);
    }
}
