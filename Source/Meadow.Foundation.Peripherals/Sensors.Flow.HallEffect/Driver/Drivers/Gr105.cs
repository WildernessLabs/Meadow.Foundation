using Meadow.Hardware;

namespace Meadow.Peripherals.Sensors.Flow;

/// <summary>
/// Driver for the GR-105 Hall effect water flow sensor.
/// </summary>
/// <remarks>
/// Configures the sensor with its factory calibration values:
/// - Scale factor: 5.5 Hz per L/min
/// </remarks>
public class Gr105 : HallEffectBase
{
    /// <summary>
    /// Initializes a new instance of the GR-105 flow sensor.
    /// </summary>
    /// <param name="pin">The digital input pin connected to the sensor's signal line.</param>
    public Gr105(IPin pin)
        : base(pin, 5.5d)
    {
    }
}
