using Meadow.Hardware;

namespace Meadow.Peripherals.Sensors.Flow;

/// <summary>
/// Driver for the GR-201 Hall effect water flow sensor.
/// </summary>
/// <remarks>
/// Configures the sensor with its factory calibration values:
/// - Scale factor: 7.5 Hz per L/min
/// </remarks>
public class Gr201 : HallEffectFlowSensor
{
    /// <summary>
    /// Initializes a new instance of the GR-201 flow sensor.
    /// </summary>
    /// <param name="pin">The digital input pin connected to the sensor's signal line.</param>
    public Gr201(IPin pin)
        : base(pin, 7.5d)
    {
    }
}
