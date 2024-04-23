using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors;

public class SimulatedCurrentSensor : SimulatedSamplingSensorBase<Current>, ICurrentSensor
{
    private Random random = new Random();

    private Current maxCurrent;
    private Current minCurrent;

    public Current? Current { get; private set; }

    public override SimulationBehavior[] SupportedBehaviors => new[] { SimulationBehavior.RandomWalk };

    public override Type ValueType => typeof(Current);

    public SimulatedCurrentSensor(Current? maxCurrent = null, Current? minCurrent = null)
    {
        this.minCurrent = minCurrent ?? new Current(0, Units.Current.UnitType.Amps);
        this.maxCurrent = maxCurrent ?? new Current(1, Units.Current.UnitType.Amps);

        Current = 0.Amps();
    }

    public override void SetSensorValue(object value)
    {
        Current = (Current)value;
    }

    protected override Current GenerateSimulatedValue(SimulationBehavior behavior)
    {
        switch (behavior)
        {
            case SimulationBehavior.RandomWalk:
                var r = random.NextDouble() * (maxCurrent.Amps - minCurrent.Amps) + minCurrent.Amps;
                this.Current = new Current(r);
                break;
        }

        return this.Current.Value;
    }
}
