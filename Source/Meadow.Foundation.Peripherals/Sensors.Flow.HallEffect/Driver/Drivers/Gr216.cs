using Meadow.Hardware;

namespace Meadow.Peripherals.Sensors.Flow;

/// <summary>
/// Driver for the GR-216 Hall effect water flow sensor.
/// </summary>
/// <remarks>
/// Configures the sensor with its factory calibration values:
/// - Scale factor: 0.2 Hz per L/min
/// </remarks>
public class Gr216 : HallEffectBase
{
    /// <summary>
    /// Initializes a new instance of the GR-216 flow sensor.
    /// </summary>
    /// <param name="pin">The digital input pin connected to the sensor's signal line.</param>
    public Gr216(IPin pin)
        : base(pin, 0.2d)
    {
    }
}
