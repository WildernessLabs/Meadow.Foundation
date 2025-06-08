using Meadow.Units;

namespace Meadow.Foundation;

public interface IProgrammableAnalogInputModule
{
    int ChannelCount { get; }
    void ConfigureChannel(ChannelConfig channelConfiguration);
    Voltage ReadChannelRaw(int channelNumber);
    Voltage Read0_10V(int channelNumber);
    Current Read0_20mA(int channelNumber);
    Current Read4_20mA(int channelNumber);
    Temperature ReadNtc(int channelNumber, double beta, Temperature referenceTemperature, Resistance resistanceAtRefTemp);
    object ReadChannelAsConfiguredUnit(int channelNumber);
}
