using Meadow.Hardware;

namespace Meadow.Peripherals.Sensors.Flow;

/// <summary>
/// Driver for the YF-B1 Hall effect water flow sensor.
/// </summary>
/// <remarks>
/// Configures the sensor with its factory calibration values:
/// - Scale factor: 11.0 Hz per L/min
/// - Offset: 0 Hz
/// </remarks>
public class YfB1 : HallEffectFlowSensor
{
    /// <summary>
    /// Initializes a new instance of the YF-B9 flow sensor.
    /// </summary>
    /// <param name="pin">The digital input pin connected to the sensor's signal line.</param>
    public YfB1(IPin pin)
        : base(pin, 11.0)
    {
    }
}
