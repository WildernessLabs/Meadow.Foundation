using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation;

/// <summary>
/// Simulated implementation of the programmable analog input module for testing purposes
/// </summary>
public class SimulatedProgrammableAnalogInputModule : ProgrammableAnalogInputModuleBase
{
    private readonly (ChannelConfig Config, double State)[] _configs;
    private readonly Voltage AdcReferenceVoltage = 3.3.Volts();
    private readonly Random _random = new Random();

    /// <summary>
    /// Gets the number of simulated analog input channels
    /// </summary>
    public int ChannelCount { get; }

    /// <summary>
    /// Creates a new SimulatedProgrammableAnalogInputModule with the specified number of channels
    /// </summary>
    /// <param name="channels">The number of simulated channels to create</param>
    public SimulatedProgrammableAnalogInputModule(int channels = 8)
        : base(channels)
    {
        ChannelCount = channels;
        _configs = new (ChannelConfig Config, double State)[channels];
        for (var c = 0; c < channels; c++)
        {
            _configs[c].Config = new ChannelConfig
            {
                ChannelType = ConfigurableAnalogInputChannelType.Voltage_0_10
            };
        }
    }

    /// <summary>
    /// Starts background simulation, randomly walking each channel's raw voltage value over time
    /// </summary>
    public void StartSimulation()
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                foreach (var channel in _configs)
                {
                    var s = ReadChannelRaw(channel.Config.ChannelNumber).Volts;

                    do
                    {
                        var delta = (_random.NextDouble() * 0.2) - 0.1;
                        s += delta;
                    } while (s < 0 || s > 3.3);

                    SetChannelRawVoltage(channel.Config.ChannelNumber, s.Volts());
                }

                await Task.Delay(1000);
            }
        });
    }

    /// <summary>
    /// Sets the raw ADC voltage for the specified channel
    /// </summary>
    /// <param name="channelNumber">The zero-based channel index to set</param>
    /// <param name="voltage">The voltage to report when the channel is read</param>
    public void SetChannelRawVoltage(int channelNumber, Voltage voltage)
    {
        _configs[channelNumber].State = voltage.Volts;
    }

    /// <inheritdoc/>
    public override void ConfigureChannel(ChannelConfig channelConfiguration)
    {
        _configs[channelConfiguration.ChannelNumber].Config = channelConfiguration;
        base.ConfigureChannel(channelConfiguration);
    }

    /// <inheritdoc/>
    public override Voltage Read0_10V(int channelNumber)
    {
        switch (_configs[channelNumber].Config.ChannelType)
        {
            case ConfigurableAnalogInputChannelType.Voltage_0_10:
                return new Voltage(ReadChannelRaw(channelNumber).Volts / AdcReferenceVoltage.Volts * 10, Voltage.UnitType.Volts);
            default:
                throw new Exception("Channel not configured for 0-10V");
        }
    }

    /// <inheritdoc/>
    public override Current Read0_20mA(int channelNumber)
    {
        switch (_configs[channelNumber].Config.ChannelType)
        {
            case ConfigurableAnalogInputChannelType.Current_0_20:
                return new Current(ReadChannelRaw(channelNumber).Volts / AdcReferenceVoltage.Volts * 20, Current.UnitType.Milliamps);
            default:
                throw new Exception("Channel not configured for 4-20mA");
        }
    }

    /// <inheritdoc/>
    public override Current Read4_20mA(int channelNumber)
    {
        switch (_configs[channelNumber].Config.ChannelType)
        {
            case ConfigurableAnalogInputChannelType.Current_4_20:
                var proportion = ReadChannelRaw(channelNumber).Volts / AdcReferenceVoltage.Volts;
                return new Current(4 + (proportion * 16), Current.UnitType.Milliamps);
            default:
                throw new Exception("Channel not configured for 4-20mA");
        }
    }

    /// <inheritdoc/>
    public override Temperature ReadNtc(int channelNumber, double beta, Temperature referenceTemperature, Resistance resistanceAtRefTemp)
    {
        switch (_configs[channelNumber].Config.ChannelType)
        {
            case ConfigurableAnalogInputChannelType.ThermistorNtc:
                return new Temperature();
            default:
                throw new Exception("Channel not configured for NTC");
        }
    }

    /// <inheritdoc/>
    public override Voltage ReadChannelRaw(int channelNumber)
    {
        return new Voltage(_configs[channelNumber].State);
    }
}
