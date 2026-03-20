using Meadow.Peripherals.Sensors;

namespace Meadow.Foundation.Sensors.Environmental;

/// <summary>
/// Represents a sensor for measuring water quality concentrations
/// </summary>
public interface IWaterQualityConcentrationsSensor : ISamplingSensor<WaterQualityConcentrations>
{
    /// <summary>
    /// Last value read from the sensor
    /// </summary>
    WaterQualityConcentrations? Concentrations { get; }
}
