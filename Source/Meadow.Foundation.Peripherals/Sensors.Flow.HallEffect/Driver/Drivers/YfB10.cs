using Meadow.Hardware;

namespace Meadow.Peripherals.Sensors.Flow;

/// <summary>
/// Driver for the YF-B10 Hall effect water flow sensor.
/// </summary>
/// <remarks>
/// Configures the sensor with its factory calibration values:
/// - Scale factor: 8.0 Hz per L/min
/// - Offset: 4 Hz
/// Note: Different data sheets differ on what the scale is on this device.  Adjust the constructor parameters if required.
/// </remarks>
public class YfB10 : HallEffectBase
{
    /// <summary>
    /// Initializes a new instance of the YF-B10 flow sensor.
    /// </summary>
    /// <param name="pin">The digital input pin connected to the sensor's signal line.</param>
    /// <param name="scale">The scale factor used to calculate flow. Defaults to 8.0 Hz per L/min.</param>
    /// <param name="offset">The offset used to calculate flow.  Defaults to 4hz</param>
    public YfB10(IPin pin, double scale = 8.0, double offset = 4)
        : base(pin, scale, offset)
    {
    }
}
