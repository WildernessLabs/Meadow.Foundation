using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Environmental;

/// <summary>
/// Represents a sensor for measuring oxidation/reduction potential
/// </summary>
public interface IRedoxPotentialSensor : ISamplingSensor<Voltage>
{
    /// <summary>
    /// Last value read from the sensor
    /// </summary>
    Voltage? Potential { get; }
}
