using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Power;

/// <summary>
/// Simulated implementation of the SPM1-X sensor for testing and development purposes.
/// Allows manual setting of current and voltage values that will be returned by sensor readings.
/// </summary>
/// <remarks>
/// This class is useful for unit testing, integration testing, and development scenarios
/// where a physical SPM1-X sensor is not available. It maintains the same interface
/// as the real sensor while allowing programmatic control of the returned values.
/// </remarks>
public class SimulatedSpm1x : ISpm1x
{
    private Current _currentValue;
    private Voltage _voltageValue;

    /// <summary>
    /// Gets the hardware revision of the simulated sensor
    /// </summary>
    /// <value>Returns a simulated hardware revision value of 1</value>
    public byte HardwareRevision { get; } = 1;

    /// <summary>
    /// Gets the Modbus address of the simulated sensor device
    /// </summary>
    /// <value>Returns a simulated Modbus address of 1</value>
    public byte ModbusAddress { get; } = 1;

    /// <summary>
    /// Gets the product model identifier of the simulated sensor
    /// </summary>
    /// <value>Returns a simulated product model value of 10 (representing SPM1-X)</value>
    public byte ProductModel { get; } = 10;

    /// <summary>
    /// Gets the serial number of the simulated sensor device
    /// </summary>
    /// <value>Returns a simulated serial number of 12345678</value>
    public int SerialNumber { get; } = 12345678;

    /// <summary>
    /// Gets the software/firmware version of the simulated sensor
    /// </summary>
    /// <value>Returns a simulated software version of 0x0100 (version 1.0)</value>
    public int SoftwareVersion { get; } = 0x0100;

    /// <summary>
    /// Gets the current measurement from the simulated sensor
    /// </summary>
    /// <value>The current value set via SetCurrent(), or 0 Amps if not set</value>
    public Current? Current => _currentValue;

    /// <summary>
    /// Gets the voltage measurement from the simulated sensor
    /// </summary>
    /// <value>The voltage value set via SetVoltage(), or 0 Volts if not set</value>
    public Voltage? Voltage => _voltageValue;

    /// <summary>
    /// Initializes a new instance of the simulated SPM1-X sensor with default values
    /// </summary>
    /// <param name="modbusAddress">Optional Modbus address for the simulated sensor (default: 1)</param>
    /// <param name="serialNumber">Optional serial number for the simulated sensor (default: 12345678)</param>
    /// <param name="softwareVersion">Optional software version for the simulated sensor (default: 0x0100)</param>
    /// <param name="productModel">Optional product model for the simulated sensor (default: 10)</param>
    /// <param name="hardwareRevision">Optional hardware revision for the simulated sensor (default: 1)</param>
    public SimulatedSpm1x(
        byte modbusAddress = 1,
        int serialNumber = 12345678,
        int softwareVersion = 0x0100,
        byte productModel = 10,
        byte hardwareRevision = 1)
    {
        ModbusAddress = modbusAddress;
        SerialNumber = serialNumber;
        SoftwareVersion = softwareVersion;
        ProductModel = productModel;
        HardwareRevision = hardwareRevision;

        // Initialize with zero values
        _currentValue = new Current(0, Units.Current.UnitType.Amps);
        _voltageValue = new Voltage(0, Units.Voltage.UnitType.Volts);
    }

    /// <summary>
    /// Sets the current value that will be returned by subsequent sensor readings
    /// </summary>
    /// <param name="current">The current value to simulate</param>
    /// <remarks>
    /// This method allows you to programmatically control what current value
    /// the simulated sensor will return when read.
    /// </remarks>
    public void SetCurrent(Current current)
    {
        _currentValue = current;
    }

    /// <summary>
    /// Sets the voltage value that will be returned by subsequent sensor readings
    /// </summary>
    /// <param name="voltage">The voltage value to simulate</param>
    /// <remarks>
    /// This method allows you to programmatically control what voltage value
    /// the simulated sensor will return when read.
    /// </remarks>
    public void SetVoltage(Voltage voltage)
    {
        _voltageValue = voltage;
    }

    /// <summary>
    /// Asynchronously reads the current measurement from the simulated sensor
    /// </summary>
    /// <returns>A task containing the current measurement set via SetCurrent()</returns>
    /// <remarks>
    /// This method simulates the async behavior of the real sensor while
    /// returning the value set via SetCurrent().
    /// </remarks>
    public async ValueTask<Current> ReadCurrent()
    {
        // Simulate some async behavior with a small delay
        await Task.Delay(1);
        return _currentValue;
    }

    /// <summary>
    /// Asynchronously reads the voltage measurement from the simulated sensor
    /// </summary>
    /// <returns>A task containing the voltage measurement set via SetVoltage()</returns>
    /// <remarks>
    /// This method simulates the async behavior of the real sensor while
    /// returning the value set via SetVoltage().
    /// </remarks>
    public async ValueTask<Voltage> ReadVoltage()
    {
        // Simulate some async behavior with a small delay
        await Task.Delay(1);
        return _voltageValue;
    }

    /// <summary>
    /// Implements the ISensor&lt;Current&gt; interface for reading current measurements
    /// </summary>
    /// <returns>A task containing the current measurement</returns>
    async Task<Current> ISensor<Current>.Read()
    {
        return await ReadCurrent();
    }

    /// <summary>
    /// Implements the ISensor&lt;Voltage&gt; interface for reading voltage measurements
    /// </summary>
    /// <returns>A task containing the voltage measurement</returns>
    async Task<Voltage> ISensor<Voltage>.Read()
    {
        return await ReadVoltage();
    }
}