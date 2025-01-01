using Meadow.Hardware;

namespace Meadow.Peripherals.Sensors.Flow;

/// <summary>
/// Driver for the YF-B10 Hall effect water flow sensor.
/// </summary>
/// <remarks>
/// The YF-B10 is a Hall effect flow sensor that outputs frequency proportional to flow rate.
/// Calibration factor is approximately 80 Hz per L/min.
/// </remarks>
public class YfB10 : HallEffectFlowSensor
{
    /// <summary>
    /// Initializes a new instance of the YF-B10 flow sensor.
    /// </summary>
    /// <param name="pin">The digital input pin connected to the sensor's signal line.</param>
    public YfB10(IPin pin)
        : base(pin, 80d)
    {
    }
}
