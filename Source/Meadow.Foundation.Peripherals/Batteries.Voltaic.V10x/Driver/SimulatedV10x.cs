using Meadow.Units;

namespace Meadow.Foundation.Batteries.Voltaic;

/// <summary>
/// Simulated implementation of the V10x battery controller for testing purposes
/// </summary>
public class SimulatedV10x : IV10x
{
    private readonly Voltage _batteryVoltage;
    private readonly Voltage _inputVoltage;
    private readonly Current _inputCurrent;
    private readonly Voltage _loadVoltage;
    private readonly Current _loadCurrent;
    private readonly Temperature _environmentTemp;
    private readonly Temperature _controllerTemp;

    /// <summary>
    /// Creates a new SimulatedV10x instance with default simulated values
    /// </summary>
    public SimulatedV10x()
    {
        _batteryVoltage = new Voltage(12.6, Voltage.UnitType.Volts);
        _inputVoltage = new Voltage(14.2, Voltage.UnitType.Volts);
        _inputCurrent = new Current(0.8, Current.UnitType.Amps);
        _loadVoltage = new Voltage(12.4, Voltage.UnitType.Volts);
        _loadCurrent = new Current(0.5, Current.UnitType.Amps);
        _environmentTemp = new Temperature(25.0, Temperature.UnitType.Celsius);
        _controllerTemp = new Temperature(35.0, Temperature.UnitType.Celsius);
    }

    /// <inheritdoc/>
    public Voltage BatteryVoltage => _batteryVoltage;

    /// <inheritdoc/>
    public Voltage InputVoltage => _inputVoltage;

    /// <inheritdoc/>
    public Current InputCurrent => _inputCurrent;

    /// <inheritdoc/>
    public Voltage LoadVoltage => _loadVoltage;

    /// <inheritdoc/>
    public Current LoadCurrent => _loadCurrent;

    /// <inheritdoc/>
    public Temperature EnvironmentTemp => _environmentTemp;

    /// <inheritdoc/>
    public Temperature ControllerTemp => _controllerTemp;
}
