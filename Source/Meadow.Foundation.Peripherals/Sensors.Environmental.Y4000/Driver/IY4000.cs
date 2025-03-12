using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Environmental;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental;

/// <summary>
/// Y4000 hardware interface
/// </summary>
public interface IY4000 :
    IWaterQualityConcentrationsSensor,
    IElectricalConductivitySensor,
    IPotentialHydrogenSensor,
    ITurbiditySensor,
    ITemperatureSensor,
    IRedoxPotentialSensor
{
    /// <summary>
    /// Get the Y4000 serial number
    /// </summary>
    /// <returns></returns>
    Task<string> GetSerialNumber();
}
