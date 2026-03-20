using Meadow.Units;

namespace Meadow.Foundation;

/// <summary>
/// Represents a programmable analog input module with configurable channel types
/// </summary>
public interface IProgrammableAnalogInputModule
{
    /// <summary>
    /// Gets the number of analog input channels on the module
    /// </summary>
    int ChannelCount { get; }

    /// <summary>
    /// Configures a channel with the specified channel configuration
    /// </summary>
    /// <param name="channelConfiguration">The configuration to apply to the channel</param>
    void ConfigureChannel(ChannelConfig channelConfiguration);

    /// <summary>
    /// Reads the raw ADC voltage from the specified channel
    /// </summary>
    /// <param name="channelNumber">The zero-based channel index to read</param>
    /// <returns>The raw voltage measured by the ADC</returns>
    Voltage ReadChannelRaw(int channelNumber);

    /// <summary>
    /// Reads a 0-10V channel and returns the scaled voltage
    /// </summary>
    /// <param name="channelNumber">The zero-based channel index to read</param>
    /// <returns>The voltage in the 0-10V range</returns>
    Voltage Read0_10V(int channelNumber);

    /// <summary>
    /// Reads a 0-20mA current loop channel
    /// </summary>
    /// <param name="channelNumber">The zero-based channel index to read</param>
    /// <returns>The current in the 0-20mA range</returns>
    Current Read0_20mA(int channelNumber);

    /// <summary>
    /// Reads a 4-20mA current loop channel
    /// </summary>
    /// <param name="channelNumber">The zero-based channel index to read</param>
    /// <returns>The current in the 4-20mA range</returns>
    Current Read4_20mA(int channelNumber);

    /// <summary>
    /// Reads an NTC thermistor channel and returns the calculated temperature
    /// </summary>
    /// <param name="channelNumber">The zero-based channel index to read</param>
    /// <param name="beta">The beta coefficient of the NTC thermistor</param>
    /// <param name="referenceTemperature">The reference temperature at which the thermistor resistance is known</param>
    /// <param name="resistanceAtRefTemp">The thermistor resistance at the reference temperature</param>
    /// <returns>The calculated temperature</returns>
    Temperature ReadNtc(int channelNumber, double beta, Temperature referenceTemperature, Resistance resistanceAtRefTemp);

    /// <summary>
    /// Reads the channel and returns the value as the unit type configured for that channel
    /// </summary>
    /// <param name="channelNumber">The zero-based channel index to read</param>
    /// <returns>The measured value as the configured unit type</returns>
    IUnit ReadChannelAsConfiguredUnit(int channelNumber);
}
