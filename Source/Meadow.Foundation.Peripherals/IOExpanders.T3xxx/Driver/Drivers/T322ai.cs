using Meadow.Hardware;
using Meadow.Modbus;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.IOExpanders;

/// <summary>
/// Driver for Temco Controls T322ai analog input module.
/// Provides 22 channels of analog input supporting both voltage (0-10V) and current (4-20mA) measurements.
/// Implements controller interfaces for creating voltage and current input ports.
/// </summary>
public partial class T322ai
    : T3xxx,
    IT322ai
{
    /// <summary>
    /// Gets the pin definitions for this T322ai module, providing access to all 22 analog input pins.
    /// </summary>
    /// <value>The PinDefinitions instance containing all available pins for this module.</value>
    public PinDefinitions Pins { get; }

    /// <summary>
    /// Initializes a new instance of the T322ai class using Modbus RTU communication.
    /// </summary>
    /// <param name="modbusRtuClient">The Modbus RTU client for serial communication.</param>
    /// <param name="moduleAddress">The Modbus address of the target T322ai module.</param>
    public T322ai(ModbusRtuClient modbusRtuClient, byte moduleAddress)
    : base(modbusRtuClient, moduleAddress)
    {
        Pins = new PinDefinitions(this);
    }

    /// <summary>
    /// Initializes a new instance of the T322ai class using Modbus TCP communication.
    /// The module address defaults to 1 for TCP connections.
    /// </summary>
    /// <param name="modbusTcpClient">The Modbus TCP client for Ethernet communication.</param>
    public T322ai(ModbusTcpClient modbusTcpClient)
        : base(modbusTcpClient)
    {
        Pins = new PinDefinitions(this);
    }

    /// <summary>
    /// Creates a current input port configured for 4-20mA measurement on the specified pin.
    /// Automatically configures the channel for current input range and sets it to auto mode.
    /// </summary>
    /// <param name="pin">The pin to configure as a current input port.</param>
    /// <returns>A new ICurrentInputPort instance configured for current measurement.</returns>
    public async Task<ICurrentInputPort> CreateCurrentInputPort(IPin pin)
    {
        var offset = (int)pin.Key;

        await WriteHoldingRegister((ushort)(T322aiRegisters.AiRange0 + offset), (ushort)AnalogInputRange.Current_4_20);
        // mode must be set to 0 (auto) in the T322i.  No idea why.
        await WriteHoldingRegister((ushort)(T322aiRegisters.AutoManual0 + offset), 0);
        // set it as an analog input 
        await WriteHoldingRegister((ushort)(T322aiRegisters.AiDiAi0 + offset), 1);

        return new T3xxx.CurrentInputPort(this, pin,
            // each input is 2 registers, the data for voltage is in the low
            (ushort)(T322aiRegisters.AiChannel0Hi + (offset * 2 + 1))
            );
    }

    /// <summary>
    /// Creates a voltage input port configured for 0-10V measurement on the specified pin.
    /// Automatically configures the channel for voltage input range and sets it to auto mode.
    /// </summary>
    /// <param name="pin">The pin to configure as a voltage input port.</param>
    /// <returns>A new IVoltageInputPort instance configured for voltage measurement.</returns>
    public async Task<IVoltageInputPort> CreateVoltageInputPort(IPin pin)
    {
        var offset = (int)pin.Key;

        await WriteHoldingRegister((ushort)(T322aiRegisters.AiRange0 + offset), (ushort)AnalogInputRange.Voltage_0_10);
        // mode must be set to 0 (auto) in the T322i.  No idea why.
        await WriteHoldingRegister((ushort)(T322aiRegisters.AutoManual0 + offset), 0);
        // set it as an analog input 
        await WriteHoldingRegister((ushort)(T322aiRegisters.AiDiAi0 + offset), 1);

        return new T3xxx.VoltageInputPort(this, pin,
            // each input is 2 registers, the data for voltage is in the low
            (ushort)(T322aiRegisters.AiChannel0Hi + (offset * 2 + 1))
            );
    }

    /// <inheritdoc/>
    public IDigitalInputPort CreateDigitalInputPort(IPin pin, ResistorMode resistorMode = ResistorMode.Disabled)
    {
        switch (resistorMode)
        {
            case ResistorMode.Disabled:
                // only valid option (really it's always internal pull-up?)
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(resistorMode), "Resistors are not supported");
        }
        var offset = (int)pin.Key;

        WriteHoldingRegister((ushort)(T322aiRegisters.AiRange0 + offset), (ushort)AnalogInputRange.Disabled)
            .GetAwaiter().GetResult();

        // mode must be set to 0 (auto)
        WriteHoldingRegister((ushort)(T322aiRegisters.AutoManual0 + offset), 0)
            .GetAwaiter().GetResult();

        // make it digital
        WriteHoldingRegister((ushort)(T322aiRegisters.AiDiAi0 + offset), 0)
            .GetAwaiter().GetResult();

        // turn the filter down (factory is 5)
        WriteHoldingRegister((ushort)(T322aiRegisters.AiFilter0 + offset), 0)
            .GetAwaiter().GetResult();

        return new T3xxx.DigitalInputPort(this, pin,
            // each input is 2 registers, the data for state is in the low
            (ushort)(T322aiRegisters.AiChannel0Hi + (offset * 2 + 1))
            );

    }

    /// <inheritdoc/>
    public ICounter CreateCounter(IPin pin, InterruptMode edge = InterruptMode.EdgeRising)
    {
        if (edge != InterruptMode.EdgeRising)
        {
            throw new ArgumentOutOfRangeException(nameof(edge), "Only EdgeRising is supported for counters");
        }

        var offset = (int)pin.Key;

        WriteHoldingRegister((ushort)(T322aiRegisters.AiRange0 + offset),
            (ushort)(offset > 11 ? AnalogInputRange.pulseCountSlow : AnalogInputRange.PulseCountFast))
            .GetAwaiter().GetResult();

        // mode must be set to 0
        WriteHoldingRegister((ushort)(T322aiRegisters.AutoManual0 + offset), 0)
            .GetAwaiter().GetResult();

        // counters are considered analogs
        WriteHoldingRegister((ushort)(T322aiRegisters.AiDiAi0 + offset), 1)
            .GetAwaiter().GetResult();

        return new T3xxx.Counter(this, pin,
            // each input is 2 registers, the data for state is in the low
            (ushort)(T322aiRegisters.AiChannel0Hi + (offset * 2 + 1))
            );
    }

    /// <inheritdoc/>
    public override async Task<int> ReadBaudRate()
    {
        var register = await ReadHoldingRegister((ushort)T322aiRegisters.BaudRate);
        return (ModuleBaudRate)register switch
        {
            ModuleBaudRate.BitRate_115200 => 115200,
            ModuleBaudRate.BitRate_57600 => 57600,
            ModuleBaudRate.BitRate_38400 => 38400,
            ModuleBaudRate.BitRate_19200 => 19200,
            ModuleBaudRate.Bitrate_9600 => 9600,
            _ => 0
        };
    }

    /// <inheritdoc/>
    public override async Task WriteBaudRate(int bitrate)
    {
        var rate = bitrate switch
        {
            115200 => ModuleBaudRate.BitRate_115200,
            57600 => ModuleBaudRate.BitRate_57600,
            38400 => ModuleBaudRate.BitRate_38400,
            19200 => ModuleBaudRate.BitRate_19200,
            9600 => ModuleBaudRate.Bitrate_9600,
            _ => throw new ArgumentOutOfRangeException()
        };

        await WriteHoldingRegister((ushort)T322aiRegisters.BaudRate, (ushort)rate);
    }

    /// <inheritdoc/>
    public override async Task WriteModbusAddress(byte newAddress)
    {
        await WriteHoldingRegister((ushort)T322aiRegisters.Address, newAddress);

        var register = await ReadHoldingRegister((ushort)T322aiRegisters.Address);
        ModbusAddress = (byte)register;
    }
}