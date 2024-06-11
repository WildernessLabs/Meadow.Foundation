using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// A simulated humidity sensor for testing and development purposes.
/// </summary>
public class SimulatedHumiditySensor : SimulatedSamplingSensorBase<RelativeHumidity>, IHumiditySensor
{
    private readonly Random random = new Random();

    /// <summary>
    /// Gets the current humidity reading.
    /// </summary>
    public RelativeHumidity? Humidity { get; private set; }

    /// <summary>
    /// Gets the type of value the sensor produces.
    /// </summary>
    public override Type ValueType => typeof(RelativeHumidity);

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedHumiditySensor"/> class.
    /// </summary>
    public SimulatedHumiditySensor()
    {
        Humidity = new RelativeHumidity(25, RelativeHumidity.UnitType.Percent);
    }

    /// <summary>
    /// Sets the sensor value.
    /// </summary>
    /// <param name="value">The value to set.</param>
    public override void SetSensorValue(object value)
    {
        Humidity = (RelativeHumidity)value;
    }

    /// <summary>
    /// Generates a simulated humidity value based on the specified behavior.
    /// </summary>
    /// <param name="behavior">The simulation behavior to use.</param>
    /// <returns>A simulated <see cref="RelativeHumidity"/> value.</returns>
    protected override RelativeHumidity GenerateSimulatedValue(SimulationBehavior behavior)
    {
        switch (behavior)
        {
            case SimulationBehavior.RandomWalk:
                var delta = random.NextDouble() * 10d - 5d;
                return new RelativeHumidity(Humidity!.Value.Percent + delta);
        }
        return Humidity!.Value;
    }
}