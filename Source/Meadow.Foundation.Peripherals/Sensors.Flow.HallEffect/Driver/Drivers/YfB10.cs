using Meadow.Hardware;

namespace Meadow.Peripherals.Sensors.Flow;

/// <summary>
/// Driver for the YF-B10 Hall effect water flow sensor.
/// </summary>
/// <remarks>
/// Configures the sensor with its factory calibration values:
/// - Scale factor: 7.5 Hz per L/min
/// - Offset: 4 Hz
/// </remarks>
public class YfB10 : HallEffectBase
{
    /// <summary>
    /// Initializes a new instance of the YF-B10 flow sensor.
    /// </summary>
    /// <param name="pin">The digital input pin connected to the sensor's signal line.</param>
    public YfB10(IPin pin)
        : base(pin, 7.5, 4)
    {
    }
}
