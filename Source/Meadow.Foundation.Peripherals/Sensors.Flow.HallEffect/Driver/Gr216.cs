using Meadow.Hardware;

namespace Meadow.Peripherals.Sensors.Flow;

/// <summary>
/// Driver for the GR-216 Hall effect water flow sensor.
/// </summary>
/// <remarks>
/// The GR-216 is a Hall effect flow sensor that outputs frequency proportional to flow rate.
/// Calibration factor is approximately 0.2 Hz per L/min.
/// </remarks>
public class Gr216 : HallEffectFlowSensor
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
