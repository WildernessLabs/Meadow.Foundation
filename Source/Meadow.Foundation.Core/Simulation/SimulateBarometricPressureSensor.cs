using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// A simulated barometric pressure sensor for testing and development purposes.
/// </summary>
public class SimulatedBarometricPressureSensor : SimulatedSamplingSensorBase<Pressure>, IBarometricPressureSensor
{
    private readonly Random random = new Random();

    /// <summary>
    /// Gets the current pressure reading.
    /// </summary>
    public Pressure? Pressure { get; private set; }

    /// <summary>
    /// Gets the type of value the sensor produces.
    /// </summary>
    public override Type ValueType => typeof(Pressure);

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulateBarometricPressureSensor"/> class.
    /// </summary>
    public SimulatedBarometricPressureSensor()
    {
        Pressure = new Pressure(1015.25, Units.Pressure.UnitType.Millibar);
    }

    /// <summary>
    /// Sets the sensor value.
    /// </summary>
    /// <param name="value">The value to set.</param>
    public override void SetSensorValue(object value)
    {
        Pressure = (Pressure)value;
    }

    /// <summary>
    /// Generates a simulated pressure value based on the specified behavior.
    /// </summary>
    /// <param name="behavior">The simulation behavior to use.</param>
    /// <returns>A simulated <see cref="Pressure"/> value.</returns>
    protected override Pressure GenerateSimulatedValue(SimulationBehavior behavior)
    {
        switch (behavior)
        {
            case SimulationBehavior.RandomWalk:
                var delta = random.NextDouble() * 10d - 5d;
                return new Pressure(Pressure!.Value.Millibar + delta);
        }
        return Pressure!.Value;
    }
}
