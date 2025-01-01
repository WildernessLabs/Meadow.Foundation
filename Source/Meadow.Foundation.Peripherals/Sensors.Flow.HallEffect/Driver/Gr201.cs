using Meadow.Hardware;

namespace Meadow.Peripherals.Sensors.Flow;

/// <summary>
/// Driver for the GR-201 Hall effect water flow sensor.
/// </summary>
/// <remarks>
/// The GR-201 is a Hall effect flow sensor that outputs frequency proportional to flow rate.
/// Calibration factor is approximately 7.5 Hz per L/min.
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
