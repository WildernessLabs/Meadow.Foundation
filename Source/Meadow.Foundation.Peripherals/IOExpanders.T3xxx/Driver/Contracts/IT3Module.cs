using Meadow.Hardware;
using System.Threading.Tasks;

namespace Meadow.Foundation.IOExpanders;

/// <summary>
/// Represents a Temco Controls T3xxx series module
/// </summary>
public interface IT3Module : IPinController
{
    /// <summary>
    /// Reads the serial number of the T3 module.
    /// </summary>
    Task<int> ReadSerialNumber();

    /// <summary>
    /// Reads the firmware version of the T3 module.
    /// </summary>
    Task<float> ReadFirmwareVersion();

    /// <summary>
    /// Reads the model identifier of the T3 module.
    /// </summary>
    Task<T3ModuleModel> ReadModel();

    /// <summary>
    /// Reads the hardware revision of the T3 module.
    /// </summary>
    Task<byte> ReadHardwareRevision();

    /// <summary>
    /// Gets the Modbus communication bitrate the sensor is configured to use
    /// </summary>
    /// <returns>A task containing the bitrate in bits per second</returns>
    Task<int> ReadBaudRate();

    /// <summary>
    /// Sets the Modbus communication bitrate the sensor is configured to use
    /// </summary>
    /// <remarks>
    /// Supports only 9600, 19200, 38400, 57600, 115200
    /// </remarks>
    Task WriteBaudRate(int bitrate);

    /// <summary>
    /// Sets the Modbus node address of the sensor
    /// </summary>
    Task WriteModbusAddress(byte newAddress);
}
