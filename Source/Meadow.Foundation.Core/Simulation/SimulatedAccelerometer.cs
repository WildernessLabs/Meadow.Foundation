using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// Represents a simulated accelerometer
/// </summary>
public class SimulatedAccelerometer : SimulatedSamplingSensorBase<Acceleration3D>, IAccelerometer
{
    /// <inheritdoc/>
    public Acceleration3D? Acceleration3D { get; private set; }

    /// <inheritdoc/>
    public override SimulationBehavior[] SupportedBehaviors => new[] { SimulationBehavior.RandomWalk };

    /// <inheritdoc/>
    public override Type ValueType => typeof(Acceleration3D);

    /// <summary>
    /// Creates a SimulatedAccelerometer instance
    /// </summary>
    public SimulatedAccelerometer()
    {
        Acceleration3D = new Acceleration3D
        {
            X = new Acceleration(0, Acceleration.UnitType.Gravity),
            Y = new Acceleration(0, Acceleration.UnitType.Gravity),
            Z = new Acceleration(1, Acceleration.UnitType.Gravity),
        };
    }

    /// <inheritdoc/>
    public override void SetSensorValue(object value)
    {
        Acceleration3D = (Acceleration3D)value;
    }

    /// <inheritdoc/>
    protected override Acceleration3D GenerateSimulatedValue(SimulationBehavior behavior)
    {
        switch (behavior)
        {
            case SimulationBehavior.RandomWalk:
                var rX = new Acceleration(GetRandomDouble(-0.5, 0.5), Acceleration.UnitType.Gravity);
                var rY = new Acceleration(GetRandomDouble(-0.5, 0.5), Acceleration.UnitType.Gravity);
                var rZ = new Acceleration(GetRandomDouble(-0.5, 0.5), Acceleration.UnitType.Gravity);
                Acceleration3D = new Acceleration3D(rX, rY, rZ);
                break;
        }

        return Acceleration3D!.Value;
    }
}
