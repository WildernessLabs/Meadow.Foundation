using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors;

public class SimulatedAccelerometer : SimulatedSamplingSensorBase<Acceleration3D>, IAccelerometer
{
    private Random random = new Random();

    public Acceleration3D? Acceleration3D { get; private set; }

    public override SimulationBehavior[] SupportedBehaviors => new[] { SimulationBehavior.RandomWalk };

    public override Type ValueType => typeof(Acceleration3D);

    public SimulatedAccelerometer()
    {
        Acceleration3D = new Acceleration3D
        {
            X = new Acceleration(0, Acceleration.UnitType.Gravity),
            Y = new Acceleration(0, Acceleration.UnitType.Gravity),
            Z = new Acceleration(1, Acceleration.UnitType.Gravity),
        };
    }

    public override void SetSensorValue(object value)
    {
        Acceleration3D = (Acceleration3D)value;
    }

    protected override Acceleration3D GenerateSimulatedValue(SimulationBehavior behavior)
    {
        switch (behavior)
        {
            case SimulationBehavior.RandomWalk:
                var rX = new Acceleration(random.NextDouble() - 0.5, Acceleration.UnitType.Gravity);
                var rY = new Acceleration(random.NextDouble() - 0.5, Acceleration.UnitType.Gravity);
                var rZ = new Acceleration(random.NextDouble() - 0.5, Acceleration.UnitType.Gravity);
                this.Acceleration3D = new Acceleration3D(rX, rY, rZ);
                break;
        }

        return this.Acceleration3D.Value;
    }
}
