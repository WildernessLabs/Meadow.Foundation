using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental;

/// <summary>
/// Defines the interface for Keller pressure transducers and transmitters.
/// </summary>
public interface IKellerTransducer
{
    /// <summary>
    /// Reads the temperature from the specified temperature channel.
    /// </summary>
    /// <param name="channel">The temperature channel to read from.</param>
    /// <returns>A task that represents the asynchronous operation. The value is the temperature reading.</returns>
    Task<Units.Temperature> ReadTemperature(TemperatureChannel channel);

    /// <summary>
    /// Reads the pressure from the specified pressure channel.
    /// </summary>
    /// <param name="channel">The pressure channel to read from.</param>
    /// <returns>A task that represents the asynchronous operation. The value is the pressure reading.</returns>
    Task<Pressure> ReadPressure(PressureChannel channel);

    /// <summary>
    /// Reads the Modbus address of the device.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the Modbus address as a byte.</returns>
    Task<byte> ReadModbusAddress();

    /// <summary>
    /// Reads the serial number of the device.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the device's serial number.</returns>
    Task<int> ReadSerialNumber();
}