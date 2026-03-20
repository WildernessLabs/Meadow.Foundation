using Meadow.Hardware;
using System.Net;
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

    /// <summary>
    /// Changes the IP address of this T3 module using UDP broadcast.
    /// Works even when the device is on a different subnet, provided <paramref name="localAddress"/>
    /// is bound to a network interface that can reach the device's subnet.
    /// </summary>
    /// <param name="localAddress">The IP address of the local network interface to bind to.</param>
    /// <param name="currentDeviceIp">The current IP address of the T3 module.</param>
    /// <param name="newIp">The new IP address to assign to the T3 module.</param>
    /// <param name="subnetMask">The subnet mask for the new IP address.</param>
    /// <param name="gateway">The default gateway for the new IP address.</param>
    /// <param name="retries">Number of times to retry if no acknowledgement is received. Defaults to 3.</param>
    /// <returns>A task that returns <c>true</c> if the device acknowledged the IP change; otherwise <c>false</c>.</returns>
    Task<bool> ChangeIpAddress(IPAddress localAddress, IPAddress currentDeviceIp, IPAddress newIp, IPAddress subnetMask, IPAddress gateway, int retries = 3);
}
