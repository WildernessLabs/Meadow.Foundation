using Meadow.Hardware;
using Meadow.Modbus;

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
    public ICurrentInputPort CreateCurrentInputPort(IPin pin)
    {
        var offset = (int)pin.Key;

        WriteHoldingRegister((ushort)(T322aiRegisters.AiRange0 + offset), (ushort)AnalogInputRange.Current_4_20).Wait();
        // mode must be set to 0 (auto) in the T322i.  No idea why.
        WriteHoldingRegister((ushort)(T322aiRegisters.AutoManual0 + offset), 0).Wait();
        // set it as an analog input 
        WriteHoldingRegister((ushort)(T322aiRegisters.AiDiAi0 + offset), 1).Wait();

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
    public IVoltageInputPort CreateVoltageInputPort(IPin pin)
    {
        var offset = (int)pin.Key;

        WriteHoldingRegister((ushort)(T322aiRegisters.AiRange0 + offset), (ushort)AnalogInputRange.Voltage_0_10).Wait();
        // mode must be set to 0 (auto) in the T322i.  No idea why.
        WriteHoldingRegister((ushort)(T322aiRegisters.AutoManual0 + offset), 0).Wait();
        // set it as an analog input 
        WriteHoldingRegister((ushort)(T322aiRegisters.AiDiAi0 + offset), 1).Wait();

        return new T3xxx.VoltageInputPort(this, pin,
            // each input is 2 registers, the data for voltage is in the low
            (ushort)(T322aiRegisters.AiChannel0Hi + (offset * 2 + 1))
            );
    }
}