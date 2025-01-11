using Meadow.Modbus;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental;

/// <summary>
/// Represents a Keller pressure transducer or transmitter that communicates via Modbus RTU.
/// </summary>
/// <remarks>
/// Keller transducers provide high-precision pressure and temperature measurements
/// for industrial and scientific applications. This implementation supports reading
/// pressure, temperature, device address, and serial number via Modbus RTU protocol.
/// </remarks>
public class KellerTransducer : IKellerTransducer
{
    private ModbusRtuClient modbusClient;
    private ushort? activePressureChannels;
    private ushort? activeTemperatureChannels;
    private byte communicationAddress;

    /// <summary>
    /// The default Modbus address for the device.
    /// </summary>
    public const int DefaultModbusAddress = 1;

    /// <summary>
    /// The default baud rate for communication with the device.
    /// </summary>
    public const int DefaultBaudRate = 9600;

    /// <summary>
    /// Creates a new instance of the KellerTransducer connected via Modbus RTU.
    /// </summary>
    /// <param name="modbus">The Modbus RTU client used to communicate with the device.</param>
    /// <param name="modbusAddress">The Modbus address of the device. Defaults to 1 if not specified.</param>
    public KellerTransducer(ModbusRtuClient modbus, byte modbusAddress = 1)
    {
        communicationAddress = modbusAddress;
        this.modbusClient = modbus;

        if (!modbus.IsConnected)
        {
            modbus.Connect();
        }
    }

    private async Task ReadConfiguration()
    {
        // the device doesn't appear to like reading > 4 registers at a time
        try
        {
            var registers = await modbusClient.ReadHoldingRegisters(communicationAddress, 0x0204, 4);
            if (registers.Length == 2)
            {
                activePressureChannels = registers[0];
                activeTemperatureChannels = registers[1];
            }
        }
        catch (Exception ex)
        {
            Resolver.Log.Warn($"Transducer initialization failure: {ex.Message}", "keller xline");
        }
    }

    /// <summary>
    /// Reads the device's Modbus Address.
    /// </summary>
    /// <remarks>
    /// The Keller Transducer can be discovered using an initial broadcast address of 250, then the actual sensor can be read using this method
    /// </remarks>
    public async Task<byte> ReadModbusAddress()
    {
        var registers = await modbusClient.ReadHoldingRegisters(communicationAddress, 0x020D, 1);
        return (byte)registers[0];
    }

    /// <inheritdoc/>
    public async Task<int> ReadSerialNumber()
    {
        var registers = await modbusClient.ReadHoldingRegisters(communicationAddress, 0x0202, 2);
        return registers.ExtractInt32();
    }

    internal Task WriteModbusAddress(byte address)
    {
        return modbusClient.WriteHoldingRegister(communicationAddress, 0x020D, address);
    }

    /// <inheritdoc/>
    public async Task<Units.Temperature> ReadTemperature(TemperatureChannel channel)
    {
        if (activeTemperatureChannels == null)
        {
            await ReadConfiguration();
        }

        if (((ushort)channel & activeTemperatureChannels) == 0)
        {
            throw new ArgumentException("Selected channel is not supported by the connected device");
        }

        ushort address = channel switch
        {
            TemperatureChannel.T => 0x006,
            TemperatureChannel.TOB1 => 0x008,
            TemperatureChannel.TOB2 => 0x00A,
            _ => throw new ArgumentException()
        };

        var r = await modbusClient.ReadHoldingRegisters(communicationAddress, address, 2);
        var temp = r.ExtractSingle();
        return new Units.Temperature(temp, Units.Temperature.UnitType.Celsius);
    }

    /// <inheritdoc/>
    public async Task<Pressure> ReadPressure(PressureChannel channel)
    {
        if (activePressureChannels == null)
        {
            await ReadConfiguration();
        }

        if (((ushort)channel & activePressureChannels) == 0)
        {
            throw new ArgumentException("Selected channel is not supported by the connected device");
        }

        ushort address = channel switch
        {
            PressureChannel.P1 => 0x0002,
            PressureChannel.P2 => 0x004,
            _ => throw new ArgumentException()
        };

        var r = await modbusClient.ReadHoldingRegisters(communicationAddress, address, 2);
        var p = r.ExtractSingle();
        return new Pressure(p, Pressure.UnitType.Bar);
    }
}
