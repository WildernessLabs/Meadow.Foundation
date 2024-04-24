using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors;

public class SimulatedHumiditySensor : SimulatedSamplingSensorBase<RelativeHumidity>, IHumiditySensor
{
    private Random random = new Random();

    public RelativeHumidity? Humidity { get; private set; }
    public override Type ValueType => typeof(RelativeHumidity);

    public SimulatedHumiditySensor()
    {
        Humidity = new RelativeHumidity(25, RelativeHumidity.UnitType.Percent);
    }

    public override void SetSensorValue(object value)
    {
        Humidity = (RelativeHumidity)value;
    }

    protected override RelativeHumidity GenerateSimulatedValue(SimulationBehavior behavior)
    {
        switch (behavior)
        {
            case SimulationBehavior.RandomWalk:
                var delta = random.NextDouble() * 10d - 5d;
                return new RelativeHumidity(Humidity.Value.Percent + delta);
        }
        return Humidity!.Value;
    }
}
