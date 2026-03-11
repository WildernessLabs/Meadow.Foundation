using Meadow.Units;

namespace Meadow.Foundation.Batteries.Voltaic;

/// <summary>
/// Represents a Voltaic Systems V10x solar charge controller
/// </summary>
public interface IV10x
{
    /// <summary>
    /// Gets the battery voltage
    /// </summary>
    Voltage BatteryVoltage { get; }

    /// <summary>
    /// Gets the solar input voltage
    /// </summary>
    Voltage InputVoltage { get; }

    /// <summary>
    /// Gets the solar input current
    /// </summary>
    Current InputCurrent { get; }

    /// <summary>
    /// Gets the load output voltage
    /// </summary>
    Voltage LoadVoltage { get; }

    /// <summary>
    /// Gets the load output current
    /// </summary>
    Current LoadCurrent { get; }

    /// <summary>
    /// Gets the ambient environment temperature
    /// </summary>
    Temperature EnvironmentTemp { get; }

    /// <summary>
    /// Gets the charge controller temperature
    /// </summary>
    Temperature ControllerTemp { get; }
}
