using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.IOExpanders;

/// <summary>
/// Driver for Temco Controls T38i8o6do mixed I/O module.
/// Provides 8 analog inputs, 8 analog outputs, and 6 digital outputs with comprehensive I/O capabilities.
/// Implements multiple controller interfaces for creating various port types.
/// </summary>
public partial class T38i8o6do
    : T3xxx,
    IDigitalOutputController,
    ICurrentInputController,
    IVoltageInputController,
    IVoltageOutputController
{
    /// <summary>
    /// Gets the pin definitions for this T38i8o6do module, providing access to all input and output pins.
    /// </summary>
    /// <value>The PinDefinitions instance containing all available pins for this module.</value>
    public PinDefinitions Pins { get; }

    /// <summary>
    /// Initializes a new instance of the T38i8o6do class using Modbus RTU communication.
    /// </summary>
    /// <param name="modbusRtuClient">The Modbus RTU client for serial communication.</param>
    /// <param name="moduleAddress">The Modbus address of the target T38i8o6do module.</param>
    public T38i8o6do(ModbusRtuClient modbusRtuClient, byte moduleAddress)
        : base(modbusRtuClient, moduleAddress)
    {
        Pins = new PinDefinitions(this);
    }

    /// <summary>
    /// Initializes a new instance of the T38i8o6do class using Modbus TCP communication.
    /// The module address defaults to 1 for TCP connections.
    /// </summary>
    /// <param name="modbusTcpClient">The Modbus TCP client for Ethernet communication.</param>
    public T38i8o6do(ModbusTcpClient modbusTcpClient)
        : base(modbusTcpClient)
    {
        Pins = new PinDefinitions(this);
    }

    /// <summary>
    /// Creates a digital output port on the specified pin with configurable initial state.
    /// Automatically configures the channel for digital output operation in manual mode.
    /// </summary>
    /// <param name="pin">The pin to configure as a digital output port.</param>
    /// <param name="initialState">The initial state to set on the digital output (default: false).</param>
    /// <param name="initialOutputType">The initial output type configuration (default: PushPull).</param>
    /// <returns>A new IDigitalOutputPort instance configured for digital output.</returns>
    public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
    {
        var offset = (int)pin.Key;

        return new T3xxx.DigitalOutputPort(this, pin,
            initialState,
            (ushort)(T38i806doRegisters.AutoManualOut0 + offset),
            (ushort)(T38i806doRegisters.OutputRange0 + offset),
            (ushort)(T38i806doRegisters.DoChannel0 + offset)
            );
    }

    /// <summary>
    /// Creates a current input port configured for 4-20mA measurement on the specified pin.
    /// Automatically configures the channel for current input range and sets it to manual mode.
    /// </summary>
    /// <param name="pin">The pin to configure as a current input port.</param>
    /// <returns>A new ICurrentInputPort instance configured for current measurement.</returns>
    public async Task<ICurrentInputPort> CreateCurrentInputPort(IPin pin)
    {
        var offset = (int)pin.Key;

        // 13 == 4-20 input
        await WriteHoldingRegister((ushort)T38i806doRegisters.AiRange0, (ushort)AnalogInputRange.Current_4_20);

        // mode must be set to 1 (manual)
        await WriteHoldingRegister((ushort)(T38i806doRegisters.AiDiAi0 + offset), 1);

        return new T3xxx.CurrentInputPort(this, pin,
            // each input is 2 registers, the data for current is in the low
            (ushort)(T38i806doRegisters.AiChannel0 + (offset * 2 + 1))
            );
    }

    /// <summary>
    /// Creates a voltage input port configured for 0-10V measurement on the specified pin.
    /// Automatically configures the channel for voltage input range and appropriate mode settings.
    /// </summary>
    /// <param name="pin">The pin to configure as a voltage input port.</param>
    /// <returns>A new IVoltageInputPort instance configured for voltage measurement.</returns>
    public async Task<IVoltageInputPort> CreateVoltageInputPort(IPin pin)
    {
        var offset = (int)pin.Key;

        // 19 == 0-10V input, 11 == 0-5V
        // not sure the utility of 0-5, since 0-10 will also do 0-5...
        await WriteHoldingRegister((ushort)(T38i806doRegisters.AiRange0 + offset), (ushort)AnalogInputRange.Voltage_0_10);
        // mode must be set to 1 (manual) in the T38o.  No idea why.
        await WriteHoldingRegister((ushort)(T38i806doRegisters.AiDiAi0 + offset), 0);

        return new T3xxx.VoltageInputPort(this, pin,
            // each input is 2 registers, the data for voltage is in the low
            (ushort)(T38i806doRegisters.AiChannel0 + (offset * 2 + 1))
            );
    }

    /// <summary>
    /// Creates a voltage output port with a specified initial voltage value on the specified pin.
    /// Automatically configures the channel for voltage output operation in manual mode.
    /// </summary>
    /// <param name="pin">The pin to configure as a voltage output port.</param>
    /// <param name="initialVoltage">The initial voltage value to set on the output.</param>
    /// <returns>A new IVoltageOutputPort instance configured for voltage output.</returns>
    public IVoltageOutputPort CreateVoltageOutputPort(IPin pin, Voltage initialVoltage)
    {
        var offset = (int)pin.Key;

        return new T3xxx.VoltageOutputPort(this, pin,
            initialVoltage,
            (ushort)(T38i806doRegisters.AutoManualOut6 + offset),
            (ushort)(T38i806doRegisters.OutputRange6 + offset),
            (ushort)(T38i806doRegisters.AoChannel0 + offset)
            );
    }

    /// <summary>
    /// Creates a voltage output port with zero initial voltage on the specified pin.
    /// This is a convenience overload that sets the initial voltage to zero.
    /// </summary>
    /// <param name="pin">The pin to configure as a voltage output port.</param>
    /// <returns>A new IVoltageOutputPort instance configured for voltage output with zero initial voltage.</returns>
    public IVoltageOutputPort CreateVoltageOutputPort(IPin pin)
    {
        return CreateVoltageOutputPort(pin, Voltage.Zero);
    }

    /// <inheritdoc/>
    public override Task<int> ReadBaudRate()
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public override Task WriteBaudRate(int bitrate)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public override Task WriteModbusAddress(byte newAddress)
    {
        throw new System.NotImplementedException();
    }
}