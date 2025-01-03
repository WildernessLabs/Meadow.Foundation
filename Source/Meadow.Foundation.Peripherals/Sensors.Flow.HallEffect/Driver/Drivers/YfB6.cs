using Meadow.Hardware;

namespace Meadow.Peripherals.Sensors.Flow;

/// <summary>
/// Driver for the YF-B10 Hall effect water flow sensor.
/// </summary>
/// <remarks>
/// Configures the sensor with its factory calibration values:
/// - Scale factor: 6.6 Hz per L/min
/// - Offset: 0 Hz
/// </remarks>
public class YfB6 : HallEffectFlowSensor
{
    /// <summary>
    /// Initializes a new instance of the YF-B6 flow sensor.
    /// </summary>
    /// <param name="pin">The digital input pin connected to the sensor's signal line.</param>
    public YfB6(IPin pin)
        : base(pin, 6.6d)
    {
    }
}
