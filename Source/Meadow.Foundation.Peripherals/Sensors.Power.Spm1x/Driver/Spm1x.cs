using Meadow.Modbus;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Power;

/// <summary>
/// Represents a Temco Controls SPM1-X Single Phase Current/Voltage sensor that communicates via Modbus RTU protocol.
/// This sensor provides real-time monitoring of electrical current and voltage measurements.
/// </summary>
/// <remarks>
/// The SPM1-X is a single-phase power monitoring device that uses Modbus RTU communication.
/// It provides accurate measurements of current (in Amps) and voltage (in Volts) for electrical monitoring applications.
/// </remarks>
public partial class Spm1x : ISpm1x
{
    /// <summary>
    /// The Modbus RTU client used for communication with the sensor
    /// </summary>
    private readonly ModbusRtuClient _modbusClient;

    /// <summary>
    /// Gets the Modbus address of the sensor device
    /// </summary>
    /// <value>The Modbus slave address used to communicate with this specific sensor instance</value>
    public byte ModbusAddress { get; private set; }

    /// <summary>
    /// Cached sensor information to avoid repeated Modbus calls
    /// </summary>
    private SensorInfo? _sensorInfo;

    /// <summary>
    /// Initializes a new instance of the SPM1-X sensor
    /// </summary>
    /// <param name="modbusClient">The Modbus RTU client instance used for communication</param>
    /// <param name="modbusAddress">The Modbus slave address of the sensor (typically 1-247)</param>
    /// <exception cref="ArgumentNullException">Thrown when modbusClient is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when modbusAddress is outside valid range (1-247)</exception>
    public Spm1x(ModbusRtuClient modbusClient, byte modbusAddress)
    {

        if (modbusAddress < 1 || modbusAddress > 254)
        {
            throw new ArgumentOutOfRangeException(nameof(modbusAddress));
        }

        _modbusClient = modbusClient;
        ModbusAddress = modbusAddress;
    }

    /// <summary>
    /// Gets the serial number of the sensor device
    /// </summary>
    /// <value>A unique 32-bit integer identifying this specific sensor unit</value>
    /// <exception cref="ModbusException">Thrown when communication with the sensor fails</exception>
    /// <exception cref="TimeoutException">Thrown when the sensor does not respond within the timeout period</exception>
    public int SerialNumber
    {
        get => GetSensorInfo().GetAwaiter().GetResult().SerialNumber;
    }

    /// <summary>
    /// Gets the software/firmware version of the sensor
    /// </summary>
    /// <value>A 16-bit integer representing the software version (e.g., 0x0102 for version 1.2)</value>
    /// <exception cref="ModbusException">Thrown when communication with the sensor fails</exception>
    /// <exception cref="TimeoutException">Thrown when the sensor does not respond within the timeout period</exception>
    public int SoftwareVersion
    {
        get => GetSensorInfo().GetAwaiter().GetResult().SoftwareVersion;
    }

    /// <summary>
    /// Gets the product model identifier of the sensor
    /// </summary>
    /// <value>A byte value indicating the specific SPM1-X model variant</value>
    /// <exception cref="ModbusException">Thrown when communication with the sensor fails</exception>
    /// <exception cref="TimeoutException">Thrown when the sensor does not respond within the timeout period</exception>
    public byte ProductModel
    {
        get => GetSensorInfo().GetAwaiter().GetResult().ProductModel;
    }

    /// <summary>
    /// Gets the hardware revision of the sensor
    /// </summary>
    /// <value>A byte value indicating the hardware revision level</value>
    /// <exception cref="ModbusException">Thrown when communication with the sensor fails</exception>
    /// <exception cref="TimeoutException">Thrown when the sensor does not respond within the timeout period</exception>
    public byte HardwareRevision
    {
        get => GetSensorInfo().GetAwaiter().GetResult().HardwareRevision;
    }

    /// <summary>
    /// Retrieves and caches sensor information from the device
    /// </summary>
    /// <returns>A task containing the sensor information structure</returns>
    /// <exception cref="ModbusException">Thrown when communication with the sensor fails</exception>
    /// <exception cref="TimeoutException">Thrown when the sensor does not respond within the timeout period</exception>
    /// <remarks>
    /// This method caches the sensor information after the first call to improve performance.
    /// The information is read from Modbus holding registers 0-9.
    /// </remarks>
    private async Task<SensorInfo> GetSensorInfo()
    {
        if (_sensorInfo == null)
        {
            if (!_modbusClient.IsConnected)
            {
                await _modbusClient.Connect();
            }

            var registers = await _modbusClient.ReadHoldingRegisters(ModbusAddress, 0, 10);

            if (registers.Length == 0)
            {
                throw new IOException("Failed to read sensor info. No registers returned.");
            }

            _sensorInfo = new SensorInfo
            {
                SerialNumber = registers[3] << 24 | registers[2] << 16 | registers[1] << 8 | registers[0],
                SoftwareVersion = registers[5] << 8 | registers[4],
                ModbusAddress = (byte)registers[6],
                ProductModel = (byte)registers[7],
                HardwareRevision = (byte)registers[8]
            };
        }

        return _sensorInfo.Value;
    }

    /// <summary>
    /// Gets the current measurement from the sensor (synchronous property)
    /// </summary>
    /// <value>The current measurement in Amps, or null if the reading fails</value>
    /// <remarks>
    /// This property provides synchronous access to the current measurement.
    /// For asynchronous access, use the ReadCurrent() method instead.
    /// </remarks>
    public Current? Current => ReadCurrent().GetAwaiter().GetResult();

    /// <summary>
    /// Gets the voltage measurement from the sensor (synchronous property)
    /// </summary>
    /// <value>The voltage measurement in Volts, or null if the reading fails</value>
    /// <remarks>
    /// This property provides synchronous access to the voltage measurement.
    /// For asynchronous access, use the ReadVoltage() method instead.
    /// </remarks>
    public Voltage? Voltage => ReadVoltage().GetAwaiter().GetResult();

    /// <summary>
    /// Asynchronously reads the current measurement from the sensor
    /// </summary>
    /// <returns>A task containing the current measurement in Amps</returns>
    /// <exception cref="ModbusException">Thrown when communication with the sensor fails</exception>
    /// <exception cref="TimeoutException">Thrown when the sensor does not respond within the timeout period</exception>
    /// <remarks>
    /// The current value is read from the sensor's holding registers and converted from 
    /// the raw register value (hundredths of an amp) to the actual current in Amps.
    /// </remarks>
    public async ValueTask<Current> ReadCurrent()
    {
        if (!_modbusClient.IsConnected)
        {
            await _modbusClient.Connect();
        }

        var registers = await _modbusClient.ReadHoldingRegisters(ModbusAddress, (ushort)Registers.Current, 1);
        if (registers.Length == 0)
        {
            throw new IOException("Failed to read current from sensor. No registers returned.");
        }
        return new Current(registers[0] / 100d, Units.Current.UnitType.Amps);
    }

    /// <summary>
    /// Implements the ISensor&lt;Current&gt; interface for reading current measurements
    /// </summary>
    /// <returns>A task containing the current measurement in Amps</returns>
    /// <exception cref="ModbusException">Thrown when communication with the sensor fails</exception>
    /// <exception cref="TimeoutException">Thrown when the sensor does not respond within the timeout period</exception>
    async Task<Current> ISensor<Current>.Read()
    {
        return await ReadCurrent();
    }

    /// <summary>
    /// Asynchronously reads the voltage measurement from the sensor
    /// </summary>
    /// <returns>A task containing the voltage measurement in Volts</returns>
    /// <exception cref="ModbusException">Thrown when communication with the sensor fails</exception>
    /// <exception cref="TimeoutException">Thrown when the sensor does not respond within the timeout period</exception>
    /// <remarks>
    /// The voltage value is read from the sensor's holding registers and converted from 
    /// the raw register value (tenths of a volt) to the actual voltage in Volts.
    /// Note: There appears to be a bug in the current implementation as it reads from the 
    /// Current register instead of the Voltage register.
    /// </remarks>
    public async ValueTask<Voltage> ReadVoltage()
    {
        if (!_modbusClient.IsConnected)
        {
            await _modbusClient.Connect();
        }

        var registers = await _modbusClient.ReadHoldingRegisters(ModbusAddress, (ushort)Registers.Voltage, 1);

        if (registers.Length == 0)
        {
            throw new IOException("Failed to read voltage from sensor. No registers returned.");
        }

        return new Voltage(registers[0] / 10d, Units.Voltage.UnitType.Volts);
    }

    /// <summary>
    /// Implements the ISensor&lt;Voltage&gt; interface for reading voltage measurements
    /// </summary>
    /// <returns>A task containing the voltage measurement in Volts</returns>
    /// <exception cref="ModbusException">Thrown when communication with the sensor fails</exception>
    /// <exception cref="TimeoutException">Thrown when the sensor does not respond within the timeout period</exception>
    async Task<Voltage> ISensor<Voltage>.Read()
    {
        return await ReadVoltage();
    }

    /// <summary>
    /// Gets the Modbus communication bitrate the sensor is configured to use
    /// </summary>
    /// <returns>A task containing the bitrate in bits per second</returns>
    public async Task<int> GetBaudRate()
    {
        if (!_modbusClient.IsConnected)
        {
            await _modbusClient.Connect();
        }

        var registers = await _modbusClient.ReadHoldingRegisters(ModbusAddress, (ushort)Registers.BaudRate, 1);

        if (registers.Length == 0)
        {
            throw new IOException("Failed to read baud rate from sensor. No registers returned.");
        }

        return (SensorBaudRate)registers[0] switch
        {
            SensorBaudRate.BitRate_19200 => 19200,
            SensorBaudRate.Bitrate_9600 => 9600,
            _ => 0
        };
    }

    /// <summary>
    /// Sets the Modbus communication bitrate the sensor is configured to use
    /// </summary>
    /// <remarks>
    /// Supports only 9600 or 19200
    /// </remarks>
    public async Task SetBaudRate(int bitrate)
    {
        var rate = bitrate switch
        {
            19200 => SensorBaudRate.BitRate_19200,
            9600 => SensorBaudRate.Bitrate_9600,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (!_modbusClient.IsConnected)
        {
            await _modbusClient.Connect();
        }

        await _modbusClient.WriteHoldingRegister(ModbusAddress, (ushort)Registers.BaudRate, (ushort)rate);
    }

    /// <summary>
    /// Sets the Modbus node address of the sensor
    /// </summary>
    /// <remarks>
    /// Supports only 9600 or 19200
    /// </remarks>
    public async Task SetModbusAddress(byte newAddress)
    {
        if (!_modbusClient.IsConnected)
        {
            await _modbusClient.Connect();
        }

        await _modbusClient.WriteHoldingRegister(ModbusAddress, (ushort)Registers.ModbusAddress, newAddress);

        var registers = await _modbusClient.ReadHoldingRegisters(ModbusAddress, (ushort)Registers.ModbusAddress, 1);
        if (registers.Length == 0)
        {
            throw new IOException("Failed to read address from sensor. No registers returned.");
        }

        ModbusAddress = (byte)registers[0];
    }
}