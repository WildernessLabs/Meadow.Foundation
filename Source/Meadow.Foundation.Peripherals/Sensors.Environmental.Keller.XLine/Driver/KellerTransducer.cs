using Meadow.Modbus;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental;

public class KellerTransducer : IKellerTransducer
{
    private ModbusRtuClient modbusClient;
    private byte modbusAddress = 250;
    private ushort activePressureChannels;
    private ushort activeTemperatureChannels;

    public int? SerialNumber { get; private set; }
    public byte ModbusAddress { get; }

    public KellerTransducer(ModbusRtuClient modbus, byte modbusAddress = 1)
    {
        ModbusAddress = modbusAddress;
        this.modbusClient = modbus;

        if (!modbus.IsConnected)
        {
            modbus.Connect();
        }

        _ = ReadConfiguration();

    }

    private bool isReady = false;

    private async Task ReadConfiguration()
    {
        // the device doesn't appear to like reading > 4 registers at a time
        var registers = await modbusClient.ReadHoldingRegisters(ModbusAddress, 0x0200, 4);
        SerialNumber = registers.ExtractInt32(2);

        registers = await modbusClient.ReadHoldingRegisters(ModbusAddress, 0x0204, 4);
        activePressureChannels = registers[0];
        activeTemperatureChannels = registers[1];

        isReady = true;
    }

    public async Task<Units.Temperature> ReadTemperature(TemperatureChannel channel)
    {
        while (!isReady)
        {
            await Task.Delay(500);
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

        var r = await modbusClient.ReadHoldingRegisters(ModbusAddress, address, 2);
        var temp = r.ExtractSingle();
        return new Units.Temperature(temp, Units.Temperature.UnitType.Celsius);
    }

    public async Task<Pressure> ReadPressure(PressureChannel channel)
    {
        while (!isReady)
        {
            await Task.Delay(500);
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

        var r = await modbusClient.ReadHoldingRegisters(ModbusAddress, address, 2);
        var p = r.ExtractSingle();
        return new Pressure(p, Pressure.UnitType.Bar);
    }
}
