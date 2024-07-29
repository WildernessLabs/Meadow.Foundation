using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Moisture;
using System;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// A simulated moisture sensor for testing and development purposes.
/// </summary>
public class SimulatedMoistureSensor : SimulatedSamplingSensorBase<double>, IMoistureSensor
{
    private readonly Random random = new();

    /// <summary>
    /// Gets the current moisture reading.
    /// </summary>
    public double? Moisture { get; private set; }

    /// <summary>
    /// Gets the type of value the sensor produces.
    /// </summary>
    public override Type ValueType => typeof(double);

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedMoistureSensor"/> class.
    /// </summary>
    public SimulatedMoistureSensor()
    {
        Moisture = 0.5d;
    }

    /// <summary>
    /// Sets the sensor value.
    /// </summary>
    /// <param name="value">The value to set.</param>
    public override void SetSensorValue(object value)
    {
        var val = Convert.ToDouble(value);
        if (val < 0) val = 0d;
        if (val > 1) val = 1.0d;

        Moisture = val;
    }

    /// <summary>
    /// Generates a simulated moisture value based on the specified behavior.
    /// </summary>
    /// <param name="behavior">The simulation behavior to use.</param>
    /// <returns>A simulated moisture value.</returns>
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
