using Meadow.Hardware;

namespace Meadow.Foundation.IOExpanders;

/// <summary>
/// Represents a 
/// Driver for Temco Controls T322ai analog input module.
/// </summary>
public interface IT322ai
    : IT3Module,
    ICurrentInputController,
    IVoltageInputController,
    IDigitalInputController,
    ICounterController
{
    /// <summary>
    /// Gets the pin definitions for this T322ai module, providing access to all 22 analog input pins.
    /// </summary>
    T322ai.PinDefinitions Pins { get; }
}
