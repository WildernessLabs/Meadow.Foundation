using Meadow.Modbus;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Batteries.Voltaic;

/// <summary>
/// Represents a Voltaic Systems V10x solar charge controller and battery
/// </summary>
public class V10x : ModbusPolledDevice
{
    private double _rawBatteryVoltage;
    private double _rawInputVoltage;
    private double _rawInputCurrent;
    private double _rawLoadVoltage;
    private double _rawLoadCurrent;
    private double _rawEnvironmentTemp;
    private double _rawControllerTemp;

    private const ushort BatteryOutputSwitchRegister = 0;

    /// <summary>
    /// The default Modbus address for the V10x device.
    /// </summary>
    public const int DefaultModbusAddress = 1;

    /// <summary>
    /// The default baud rate for communication with the V10x device.
    /// </summary>
    public const int DefaultBaudRate = 9600;

    /// <summary>
    /// Gets the battery voltage.
    /// </summary>
    public Voltage BatteryVoltage => new Voltage(_rawBatteryVoltage, Voltage.UnitType.Volts);

    /// <summary>
    /// Gets the input voltage.
    /// </summary>
    public Voltage InputVoltage => new Voltage(_rawInputVoltage, Voltage.UnitType.Volts);

    /// <summary>
    /// Gets the input current.
    /// </summary>
    public Current InputCurrent => new Current(_rawInputCurrent, Current.UnitType.Amps);

    /// <summary>
    /// Gets the load voltage.
    /// </summary>
    public Voltage LoadVoltage => new Voltage(_rawLoadVoltage, Voltage.UnitType.Volts);

    /// <summary>
    /// Gets the load current.
    /// </summary>
    public Current LoadCurrent => new Current(_rawLoadCurrent, Current.UnitType.Amps);

    /// <summary>
    /// Gets the environment temperature.
    /// </summary>
    public Temperature EnvironmentTemp => new Temperature(_rawEnvironmentTemp, Temperature.UnitType.Celsius);

    /// <summary>
    /// Gets the controller temperature.
    /// </summary>
    public Temperature ControllerTemp => new Temperature(_rawControllerTemp, Temperature.UnitType.Celsius);

    /// <summary>
    /// Creates a new V10x instance.
    /// </summary>
    /// <param name="client">The modbus client</param>
    /// <param name="modbusAddress">The modbuss address</param>
    /// <param name="refreshPeriod">The refresh period</param>
    public V10x(
        ModbusClientBase client,
        byte modbusAddress = DefaultModbusAddress,
        TimeSpan? refreshPeriod = null)
        : base(client, modbusAddress, refreshPeriod)
    {
        MapInputRegistersToField(
            startRegister: 0x30a0,
            registerCount: 1,
            fieldName: nameof(_rawBatteryVoltage),
            conversionFunction: ConvertRegisterToRawValue
            );

        MapInputRegistersToField(
            startRegister: 0x304e,
            registerCount: 1,
            fieldName: nameof(_rawInputVoltage),
            conversionFunction: ConvertRegisterToRawValue
            );

        MapInputRegistersToField(
            startRegister: 0x304f,
            registerCount: 1,
            fieldName: nameof(_rawInputCurrent),
            conversionFunction: ConvertRegisterToRawValue
            );

        MapInputRegistersToField(
            startRegister: 0x304a,
            registerCount: 1,
            fieldName: nameof(_rawLoadVoltage),
            conversionFunction: ConvertRegisterToRawValue
            );

        MapInputRegistersToField(
            startRegister: 0x304b,
            registerCount: 1,
            fieldName: nameof(_rawLoadCurrent),
            conversionFunction: ConvertRegisterToRawValue
            );

        MapInputRegistersToField(
            startRegister: 0x30a2,
            registerCount: 1,
            fieldName: nameof(_rawEnvironmentTemp),
            conversionFunction: ConvertRegisterToRawValue
            );

        MapInputRegistersToField(
            startRegister: 0x3037,
            registerCount: 1,
            fieldName: nameof(_rawControllerTemp),
            conversionFunction: ConvertRegisterToRawValue
            );
    }

    /// <summary>
    /// Sets the battery output switch state.
    /// </summary>
    public bool BatteryOutput
    {
        set => _ = WriteCoil(BatteryOutputSwitchRegister, value);
    }

    private object ConvertRegisterToRawValue(ushort[] registers)
    {
        // value is one register in 1/100 of a unit
        return registers[0] / 100d;
    }

    /// <summary>
    /// Reads the device's Modbus Address.
    /// </summary>
    /// <remarks>
    /// The device can be discovered using an initial broadcast address of 254, then the actual sensor can be read using this method
    /// </remarks>
    public async Task<byte> ReadModbusAddress()
    {
        var registers = await base.ReadHoldingRegisters(9020, 1);
        return (byte)registers[0];
    }

    /// <summary>
    /// Reads the device's Modbus Address.
    /// </summary>
    /// <remarks>
    /// The device can be discovered using an initial broadcast address of 254, then the actual sensor can be read using this method
    /// </remarks>
    public async Task WriteModbusAddress(byte address)
    {
        await base.WriteHoldingRegister(9020, address);
        base.BusAddress = address;
    }
}
