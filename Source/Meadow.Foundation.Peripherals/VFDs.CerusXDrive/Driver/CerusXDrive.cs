using Meadow.Modbus;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.VFDs;

/// <summary>
/// Represents a Franklin Electric CerusXDrive Variable Frequency Drive controller.
/// Provides methods to communicate with the drive via Modbus RTU protocol.
/// </summary>
public class CerusXDrive : IXDrive
{
    private readonly ModbusRtuClient _modbus;
    private readonly byte _busAddress;

    /// <summary>
    /// Initializes a new instance of the CerusXDrive class.
    /// </summary>
    /// <param name="modbusRtuClient">The Modbus RTU client used for communication.</param>
    /// <param name="busAddress">The bus address of the CerusXDrive device.</param>
    public CerusXDrive(ModbusRtuClient modbusRtuClient, byte busAddress)
    {
        _modbus = modbusRtuClient;
        _busAddress = busAddress;
    }

    /// <summary>
    /// Establishes a connection to the CerusXDrive device.
    /// </summary>
    /// <returns>A task representing the asynchronous connection operation.</returns>
    public Task Connect()
    {
        return _modbus.Connect();
    }

    /// <summary>
    /// Disconnects from the CerusXDrive device.
    /// </summary>
    public void Disconnect()
    {
        _modbus.Disconnect();
    }

    /// <summary>
    /// Reads the current error codes from the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the error code.</returns>
    public async Task<ushort> ReadErrorCodes()
    {
        var f = await ReadRegister(Registers.ErrorCode);
        return f;
    }

    /// <summary>
    /// Reads the current operational status of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the operational status code.</returns>
    public async Task<ushort> ReadOperationalStatus()
    {
        var f = await ReadRegister(Registers.OperationStatus);
        return f;
    }

    /// <summary>
    /// Reads the current output frequency of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the output frequency in Hertz.</returns>
    public async Task<Frequency> ReadOutputFrequency()
    {
        var f = await ReadRegister(Registers.OutputFrequency);
        return new Frequency(f / 100d, Frequency.UnitType.Hertz);
    }

    /// <summary>
    /// Reads the current output current of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the output current in Amps.</returns>
    public async Task<Current> ReadOutputCurrent()
    {
        var f = await ReadRegister(Registers.OutputCurrent);
        return new Current(f / 10d, Current.UnitType.Amps);
    }

    /// <summary>
    /// Reads the current DC bus voltage of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the DC bus voltage in Volts.</returns>
    public async Task<Voltage> ReadDCBusVoltage()
    {
        var f = await ReadRegister(Registers.DCBusVoltage);
        return new Voltage(f / 10d, Voltage.UnitType.Volts);
    }

    /// <summary>
    /// Reads the current output voltage of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the output voltage in Volts.</returns>
    public async Task<Voltage> ReadOutputVoltage()
    {
        var f = await ReadRegister(Registers.OutputVoltage);
        return new Voltage(f / 10d, Voltage.UnitType.Volts);
    }

    /// <summary>
    /// Reads the current IGBT (Insulated Gate Bipolar Transistor) temperature of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the IGBT temperature in Celsius.</returns>
    public async Task<Temperature> ReadIGBTTemperature()
    {
        var f = await ReadRegister(Registers.IGBTTemperature);
        return new Temperature(f / 10d, Temperature.UnitType.Celsius);
    }

    /// <summary>
    /// Reads the current ambient temperature around the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the ambient temperature in Celsius.</returns>
    public async Task<Temperature> ReadAmbientTemperature()
    {
        var f = await ReadRegister(Registers.AmbientTemperature);
        return new Temperature(f / 10d, Temperature.UnitType.Celsius);
    }

    /// <summary>
    /// Reads the current drive status of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the drive status code.</returns>
    public async Task<ushort> ReadDriveStatus()
    {
        return await ReadRegister(Registers.DriveStatus);
    }

    /// <summary>
    /// Reads the current control mode of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the control mode code.</returns>
    public async Task<ushort> ReadControlMode()
    {
        return await ReadRegister(Registers.ControlMode);
    }

    /// <summary>
    /// Reads the current digital output status of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the digital output status code.</returns>
    public async Task<ushort> ReadDigitalOutputStatus()
    {
        return await ReadRegister(Registers.DigitalOutputStatus);
    }

    /// <summary>
    /// Reads the current digital input status of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the digital input status code.</returns>
    public async Task<ushort> ReadDigitalInputStatus()
    {
        return await ReadRegister(Registers.DigitalInputStatus);
    }

    /// <summary>
    /// Writes a value to the specified register on the CerusXDrive.
    /// </summary>
    /// <param name="register">The register to write to.</param>
    /// <param name="value">The value to write to the register.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    private async Task WriteRegister(Registers register, ushort value)
    {
        await _modbus.WriteHoldingRegister(_busAddress, (ushort)register, value);
    }

    /// <summary>
    /// Reads a value from the specified register on the CerusXDrive.
    /// </summary>
    /// <param name="register">The register to read from.</param>
    /// <returns>A task that represents the asynchronous operation. The value is the register value.</returns>
    private async Task<ushort> ReadRegister(Registers register)
    {
        var r = await _modbus.ReadHoldingRegisters(_busAddress, (ushort)register, 1);
        return r[0];
    }
}