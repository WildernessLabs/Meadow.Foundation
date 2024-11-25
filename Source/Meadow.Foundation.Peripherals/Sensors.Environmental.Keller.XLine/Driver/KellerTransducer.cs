using Meadow.Modbus;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental;

public class KellerTransducer : IKellerTransducer
{
    private ModbusRtuClient modbusClient;
    private ushort? activePressureChannels;
    private ushort? activeTemperatureChannels;
    private byte communicationAddress;

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
            activePressureChannels = registers[0];
            activeTemperatureChannels = registers[1];
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

    public async Task<int> ReadSerialNumber()
    {
        var registers = await modbusClient.ReadHoldingRegisters(communicationAddress, 0x0202, 2);
        return registers.ExtractInt32();
    }

    public Task WriteModbusAddress(byte address)
    {
        return modbusClient.WriteHoldingRegister(communicationAddress, 0x020D, address);
    }

    public async Task<Units.Temperature> ReadTemperature(TemperatureChannel channel)
    {
        Resolver.Log.Info($"Reading transducer temp");

        var count = 6;

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
