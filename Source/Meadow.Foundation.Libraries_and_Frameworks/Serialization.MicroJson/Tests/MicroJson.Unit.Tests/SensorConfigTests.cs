using Meadow.Foundation.Serialization;
using System;
using Xunit;

namespace Unit.Tests;

public class SensorConfigTests
{
    [Fact]
    public void DeserializePuzzlesAsArrayTest()
    {
        var json = Inputs.GetInputResource("sensors.json");
        var result = MicroJson.Deserialize<SensorConfiguration>(json);

        Assert.NotNull(result);
    }
}

public class SensorConfiguration
{
    public DigitalInputConfig[] DigitalInputs { get; set; } = Array.Empty<DigitalInputConfig>();
    public AnalogModuleConfig? ConfigurableAnalogs { get; set; }
    public FrequencyInputConfig[] FrequencyInputs { get; set; } = Array.Empty<FrequencyInputConfig>();
    public ModbusDeviceConfig[] ModbusDevices { get; set; } = Array.Empty<ModbusDeviceConfig>();
    public T322iConfiguration? T322iInputs { get; set; }
}

public interface IIntervalReadSensor
{
    int SenseIntervalSeconds { get; }
}

public class FrequencyInputConfig : IIntervalReadSensor
{
    public int ChannelNumber { get; set; }
    public string UnitType { get; set; }
    public double Scale { get; set; }
    public double Offset { get; set; }
    public string Name { get; set; }
    public bool IsSimulated { get; set; }
    public int SenseIntervalSeconds { get; set; }
}

public class ModbusDeviceConfig : IIntervalReadSensor
{
    public string Driver { get; set; }
    public int Address { get; set; }
    public string Name { get; set; }
    public int SenseIntervalSeconds { get; set; }
    public bool IsSimulated { get; set; }
}

public class T322iConfiguration
{
    public int ModbusAddress { get; set; }
    public bool IsSimulated { get; set; }
    public ExtendedChannelConfig[] Channels { get; set; }
}

public class DigitalInputConfig : IIntervalReadSensor
{
    public int ChannelNumber { get; set; }
    public string Name { get; set; }
    public bool IsSimulated { get; set; }
    public int SenseIntervalSeconds { get; set; }
}

public class AnalogModuleConfig
{
    public bool IsSimulated { get; set; }
    public ExtendedChannelConfig[] Channels { get; set; }
}

public class ExtendedChannelConfig : ChannelConfig
{
    public int SenseIntervalSeconds { get; set; }
}

public class ChannelConfig
{
    public int ChannelNumber { get; set; }
    public ConfigurableAnalogInputChannelType ChannelType { get; set; }
    public double Scale { get; set; } = 1.0;
    public double Offset { get; set; } = 0.0;
    public string UnitType { get; set; }
    public string Name { get; set; }
}

public enum ConfigurableAnalogInputChannelType
{
    Voltage_0_10,
    Current_4_20,
    Current_0_20,
    ThermistorNtc
}


