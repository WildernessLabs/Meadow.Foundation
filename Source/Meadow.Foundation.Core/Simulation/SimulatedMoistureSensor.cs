using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Moisture;
using System;

namespace Meadow.Foundation.Sensors;

public class SimulatedMoistureSensor : SimulatedSamplingSensorBase<double>, IMoistureSensor
{
    private Random random = new Random();

    public double? Moisture { get; private set; }
    public override Type ValueType => typeof(double);

    public SimulatedMoistureSensor()
    {
        Moisture = 0.5d;
    }

    public override void SetSensorValue(object value)
    {
        var val = Convert.ToDouble(value);
        if (val < 0) val = 0d;
        if (val > 1) val = 1.0d;

        Moisture = val;
    }

    protected override double GenerateSimulatedValue(SimulationBehavior behavior)
    {
        switch (behavior)
        {
            case SimulationBehavior.RandomWalk:
                var delta = random.NextDouble() / 100d - 0.5;
                return Moisture!.Value + delta;
        }
        return Moisture ?? 0;
    }
}
