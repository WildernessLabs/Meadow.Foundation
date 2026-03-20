using Meadow.Modbus;

namespace T3ConfigConsole.Devices;

/// <summary>
/// Thin Modbus client for the T38i8o6do targeted at the configuration console.
/// Works directly against registers — no higher-level port abstraction needed here.
/// </summary>
public class T38i8o6doConsoleClient
{
    private readonly ModbusClientBase _client;
    private readonly byte _unitId;

    public T38i8o6doConsoleClient(ModbusClientBase client, byte unitId = 1)
    {
        _client = client;
        _unitId = unitId;
    }

    public async Task<DeviceInfo> ReadDeviceInfoAsync()
    {
        var r = await _client.ReadHoldingRegisters(_unitId, T38i8o6doRegisters.SerialNumber, 9);

        var info = new DeviceInfo
        {
            SerialNumber     = r[0] | r[1] << 8 | r[2] << 16 | r[3] << 24,
            FirmwareVersion  = r[4] / 10f,
            ModbusAddress    = (byte)r[6],
            ProductModel     = r[7],
            HardwareRevision = (byte)r[8]
        };

        if (_client is ModbusTcpClient tcp)
        {
            info.IpAddress = tcp.Destination;
        }

        return info;
    }

    public async Task<List<InputPoint>> ReadAllInputsAsync(int model)
    {
        if (model != 0x002C && model != 44) // T38o
        {
            return new List<InputPoint>();
        }

        var aiValues   = await _client.ReadHoldingRegisters(_unitId, T38i8o6doRegisters.AiChannelBase, 16);
        var autoManual = await _client.ReadHoldingRegisters(_unitId, T38i8o6doRegisters.AutoManualInBase, 8);
        var aiDiAi     = await _client.ReadHoldingRegisters(_unitId, T38i8o6doRegisters.AiDiAiBase, 8);
        var status     = await _client.ReadHoldingRegisters(_unitId, T38i8o6doRegisters.InputStatusBase, 8);
        var ranges     = await _client.ReadHoldingRegisters(_unitId, T38i8o6doRegisters.InputRangeBase, 8);

        var points = new List<InputPoint>(8);
        for (int i = 0; i < 8; i++)
        {
            points.Add(new InputPoint
            {
                Index          = i,
                ValueRegister  = (ushort)(T38i8o6doRegisters.AiChannelBase + i * 2 + 1), // High reg of pair
                TypeRegister   = (ushort)(T38i8o6doRegisters.AiDiAiBase + i),
                ModeRegister   = (ushort)(T38i8o6doRegisters.AutoManualInBase + i),
                RangeRegister  = (ushort)(T38i8o6doRegisters.InputRangeBase + i),
                StatusRegister = (ushort)(T38i8o6doRegisters.InputStatusBase + i),

                IsAnalog       = aiDiAi[i] != 0,
                IsManual       = autoManual[i] != 0,
                RangeCode      = ranges[i],
                RawValue       = aiValues[i * 2 + 1],
                RawStatus      = status[i]
            });
        }
        return points;
    }

    public Task WriteInputTypeAsync(int channelIndex, bool analog) =>
        _client.WriteHoldingRegister(
            _unitId,
            (ushort)(T38i8o6doRegisters.AiDiAiBase + channelIndex),
            (ushort)(analog ? 1 : 0)); // 0 = Digital, 1 = Analog (Manual/Current mode usually sets to 1)

    public Task WriteInputModeAsync(int channelIndex, bool manual) =>
        _client.WriteHoldingRegister(
            _unitId,
            (ushort)(T38i8o6doRegisters.AutoManualInBase + channelIndex),
            (ushort)(manual ? 1 : 0));

    public Task WriteInputRangeAsync(int channelIndex, ushort rangeCode) =>
        _client.WriteHoldingRegister(
            _unitId,
            (ushort)(T38i8o6doRegisters.InputRangeBase + channelIndex),
            rangeCode);


    public async Task<int> ReadBaudRateAsync()
    {
        var register = await _client.ReadHoldingRegisters(_unitId, 15, 1);
        return register[0] switch
        {
            4 => 115200,
            3 => 57600,
            2 => 38400,
            1 => 19200,
            0 => 9600,
            _ => 9600
        };
    }

    public Task WriteBaudRateAsync(int bitrate)
    {
        ushort rate = bitrate switch
        {
            115200 => 4,
            57600 => 3,
            38400 => 2,
            19200 => 1,
            9600 => 0,
            _ => throw new ArgumentException("Invalid baud rate")
        };
        return _client.WriteHoldingRegister(_unitId, 15, rate);
    }

    public Task WriteModbusAddressAsync(byte newAddress) =>
        _client.WriteHoldingRegister(_unitId, 6, newAddress);

    public async Task<ushort> ReadInputValueAsync(int channelIndex)
    {
        var addr = (ushort)(T38i8o6doRegisters.AiChannelBase + channelIndex * 2 + 1);
        var r = await _client.ReadHoldingRegisters(_unitId, addr, 1);
        return r[0];
    }

    public async Task<List<OutputPoint>> ReadAllOutputsAsync()
    {
        // Analog outputs (100-107)
        var aoValues   = await _client.ReadHoldingRegisters(_unitId, T38i8o6doRegisters.AoChannelBase,     T38i8o6doRegisters.OutputCount);
        // Digital outputs (108-113)
        var doValues   = await _client.ReadHoldingRegisters(_unitId, T38i8o6doRegisters.DoChannelBase,     6);
        
        var autoManual = await _client.ReadHoldingRegisters(_unitId, T38i8o6doRegisters.AutoManualOutBase, T38i8o6doRegisters.OutputCount);
        var status     = await _client.ReadHoldingRegisters(_unitId, T38i8o6doRegisters.OutputStatusBase,  T38i8o6doRegisters.OutputCount);
        var ranges     = await _client.ReadHoldingRegisters(_unitId, T38i8o6doRegisters.OutputRangeBase,   T38i8o6doRegisters.OutputCount);
        var adTypes    = await _client.ReadHoldingRegisters(_unitId, T38i8o6doRegisters.OutputAdBase,      T38i8o6doRegisters.OutputCount);

        var points = new List<OutputPoint>();
        
        // Add AOs
        for (int i = 0; i < T38i8o6doRegisters.OutputCount; i++)
        {
            points.Add(new OutputPoint
            {
                Index     = i,
                IsAnalog  = adTypes[i] == 1,
                IsManual  = autoManual[i] != 0,
                RangeCode = (byte)(ranges[i] & 0xFF),
                RawValue  = aoValues[i],
                RawMode   = autoManual[i],
                RawRange  = ranges[i],
                RawStatus = status[i]
            });
        }

        // Add DOs (dedicated relays)
        for (int i = 0; i < 6; i++)
        {
            points.Add(new OutputPoint
            {
                Index     = 100 + i, // Use a virtual index for DOs
                IsAnalog  = false,
                IsManual  = true, // DOs are effectively manual
                RangeCode = 0,
                RawValue  = doValues[i],
                RawMode   = 1,
                RawRange  = 0,
                RawStatus = 0
            });
        }

        return points;
    }

    public Task WriteOutputModeAsync(int channelIndex, bool manual) =>
        _client.WriteHoldingRegister(
            _unitId,
            (ushort)(T38i8o6doRegisters.AutoManualOutBase + channelIndex),
            (ushort)(manual ? 1 : 0));

    public Task WriteOutputValueAsync(int channelIndex, ushort value)
    {
        ushort reg;
        if (channelIndex >= 100) // It's a dedicated DO
        {
            reg = (ushort)(T38i8o6doRegisters.DoChannelBase + (channelIndex - 100));
        }
        else // It's an AO channel
        {
            reg = (ushort)(T38i8o6doRegisters.AoChannelBase + channelIndex);
        }

        return _client.WriteHoldingRegister(_unitId, reg, value);
    }
}
