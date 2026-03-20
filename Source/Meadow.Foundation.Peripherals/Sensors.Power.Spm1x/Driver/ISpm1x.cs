using Meadow.Peripherals.Sensors;

namespace Meadow.Foundation.Sensors.Power;

/// <summary>
/// Represents a sensor interface that provides access to current and voltage measurements, as well as hardware and
/// software identification details.
/// </summary>
public interface ISpm1x : ICurrentSensor, IVoltageSensor
{
    /// <summary>
    /// Gets the hardware revision of the sensor
    /// </summary>
    byte HardwareRevision { get; }
    /// <summary>
    /// Gets the Modbus address of the sensor device
    /// </summary>
    byte ModbusAddress { get; }
    /// <summary>
    /// Gets the product model identifier of the sensor
    /// </summary>
    byte ProductModel { get; }
    /// <summary>
    /// Gets the serial number of the sensor device
    /// </summary>
    int SerialNumber { get; }
    /// <summary>
    /// Gets the software/firmware version of the sensor
    /// </summary>
    int SoftwareVersion { get; }
}