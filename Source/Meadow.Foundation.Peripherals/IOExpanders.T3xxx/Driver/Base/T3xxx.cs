using Meadow.Hardware;
using Meadow.Modbus;
using System.Threading.Tasks;

namespace Meadow.Foundation.IOExpanders;

/// <summary>
/// Abstract base class for Temco Controls T3xxx series modules.
/// Provides common functionality for communicating with T3 modules via Modbus protocol.
/// Implements IPinController to manage module pins and I/O operations.
/// </summary>
public abstract partial class T3xxx
    : IT3Module, IPinController
{
    private T3ModuleModel? _model = null;
    private int? _serialNumber = null;
    private float? _firmware = null;
    private byte? _hardwareRevision = null;

    /// <summary>
    /// Gets the Modbus client used for communication with the T3 module.
    /// </summary>
    /// <value>The IModbusBusClient instance for protocol communication.</value>
    protected IModbusBusClient ModbusClient { get; }

    /// <summary>
    /// Gets the Modbus address of this T3 module on the bus.
    /// </summary>
    /// <value>The byte address used to identify this module in Modbus communications.</value>
    public byte ModbusAddress { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the T3xxx class using Modbus RTU communication.
    /// </summary>
    /// <param name="modbusRtuClient">The Modbus RTU client for serial communication.</param>
    /// <param name="moduleAddress">The Modbus address of the target T3 module.</param>
    protected T3xxx(ModbusRtuClient modbusRtuClient, byte moduleAddress)
    {
        ModbusClient = modbusRtuClient;
        ModbusAddress = moduleAddress;
    }

    /// <summary>
    /// Initializes a new instance of the T3xxx class using Modbus TCP communication.
    /// The module address defaults to 1 for TCP connections.
    /// </summary>
    /// <param name="modbusTcpClient">The Modbus TCP client for Ethernet communication.</param>
    public T3xxx(ModbusTcpClient modbusTcpClient)
    {
        ModbusClient = modbusTcpClient;
        ModbusAddress = 1;
    }

    /// <summary>
    /// Reads and parses the module's header registers containing identification and configuration data.
    /// This method caches the results to avoid repeated register reads.
    /// </summary>
    /// <returns>A task representing the asynchronous read operation.</returns>
    private async Task ReadAndParseHeaderRegisters()
    {
        if (_model == null)
        {
            var registers = await ModbusClient.ReadHoldingRegisters(ModbusAddress, 0, 9);
            _serialNumber = registers[0] | registers[1] << 8 | registers[2] << 16 | registers[3] << 24;
            _firmware = (registers[4] / 10f);
            ModbusAddress = (byte)registers[6];
            _model = (T3ModuleModel)registers[7];
            _hardwareRevision = (byte)registers[8];
        }
    }

    /// <summary>
    /// Writes a value to a holding register on the T3 module.
    /// </summary>
    /// <param name="register">The register address to write to.</param>
    /// <param name="value">The 16-bit value to write to the register.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    internal async Task WriteHoldingRegister(ushort register, ushort value)
    {
        await ModbusClient.WriteHoldingRegister(this.ModbusAddress, register, value);
    }

    /// <summary>
    /// Reads a value from a holding register on the T3 module.
    /// </summary>
    /// <param name="register">The register address to read from.</param>
    /// <returns>A task containing the 16-bit value read from the register.</returns>
    internal async Task<ushort> ReadHoldingRegister(ushort register)
    {
        return (await ModbusClient.ReadHoldingRegisters(this.ModbusAddress, register, 1))[0];
    }

    /// <summary>
    /// Reads the current Modbus address of the T3 module.
    /// </summary>
    /// <returns>A ValueTask containing the module's Modbus address.</returns>
    public async ValueTask<byte> ReadModbusAddress()
    {
        await ReadAndParseHeaderRegisters();
        return ModbusAddress;
    }

    /// <summary>
    /// Reads the serial number of the T3 module.
    /// </summary>
    /// <returns>A task containing the module's unique serial number.</returns>
    public async Task<int> ReadSerialNumber()
    {
        await ReadAndParseHeaderRegisters();
        return _serialNumber!.Value;
    }

    /// <summary>
    /// Reads the firmware version of the T3 module.
    /// </summary>
    /// <returns>A task containing the module's firmware version as a float value.</returns>
    public async Task<float> ReadFirmwareVersion()
    {
        await ReadAndParseHeaderRegisters();
        return _firmware!.Value;
    }

    /// <summary>
    /// Reads the model identifier of the T3 module.
    /// </summary>
    /// <returns>A task containing the T3ModuleModel enum value identifying the specific module type.</returns>
    public async Task<T3ModuleModel> ReadModel()
    {
        await ReadAndParseHeaderRegisters();
        return _model!.Value;
    }

    /// <summary>
    /// Reads the hardware revision of the T3 module.
    /// </summary>
    /// <returns>A task containing the module's hardware revision number.</returns>
    public async Task<byte> ReadHardwareRevision()
    {
        await ReadAndParseHeaderRegisters();
        return _hardwareRevision!.Value;
    }

    /// <inheritdoc/>
    public abstract Task<int> ReadBaudRate();

    /// <inheritdoc/>
    public abstract Task WriteBaudRate(int bitrate);

    /// <inheritdoc/>
    public abstract Task WriteModbusAddress(byte newAddress);
}