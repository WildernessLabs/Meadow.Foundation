using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental;

/// <summary>
/// Simulated implementation of the IKellerTransducer interface.
/// This class provides mock functionality for testing or development purposes without hardware.
/// </summary>
public class SimulatedKellerTransducer : IKellerTransducer
{
    /// <summary>
    /// Reads the Modbus address of the transducer.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the Modbus address as a byte.</returns>
    public Task<byte> ReadModbusAddress()
    {
        return Task.FromResult((byte)255);
    }

    /// <summary>
    /// Reads the pressure from the specified channel.
    /// </summary>
    /// <param name="channel">The pressure channel to read from.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the pressure as a <see cref="Pressure"/> object.</returns>
    public Task<Pressure> ReadPressure(PressureChannel channel)
    {
        return Task.FromResult(new Pressure(123, Pressure.UnitType.Millibar));
    }

    /// <summary>
    /// Reads the serial number of the transducer.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the serial number as an integer.</returns>
    public Task<int> ReadSerialNumber()
    {
        return Task.FromResult(12345678);
    }

    /// <summary>
    /// Reads the temperature from the specified channel.
    /// </summary>
    /// <param name="channel">The temperature channel to read from.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the temperature as a <see cref="Units.Temperature"/> object.</returns>
    public Task<Units.Temperature> ReadTemperature(TemperatureChannel channel)
    {
        return Task.FromResult(new Units.Temperature(42.1, Units.Temperature.UnitType.Fahrenheit));
    }
}