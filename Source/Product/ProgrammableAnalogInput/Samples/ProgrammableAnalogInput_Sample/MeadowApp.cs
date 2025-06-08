using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace ProgrammableAnalogInput_Sample;

public class MeadowApp : App<F7CoreComputeV2>
{
    //<!=SNIP=>
    private ProgrammableAnalogInputModule module;

    public override async Task Initialize()
    {
        Resolver.Log.Info("Initialize...");

        var bus = Device.CreateI2cBus();

        module = new ProgrammableAnalogInputModule(
            bus,
            0x10,
            0x20,
            0x21);
    }

    public override async Task Run()
    {
        Resolver.Log.Info("Run...");
    }

    public async Task Test4_20()
    {
        for (var i = 0; i < 8; i++)
        {
            module.ConfigureChannel(new ChannelConfig
            {
                ChannelNumber = i,
                ChannelType = ConfigurableAnalogInputChannelType.Current_4_20,
                UnitType = "Temperature",
                Scale = 3.4725, //0-100F, but scale/offs in C
                Offset = -31.67
            });
        }

        while (true)
        {
            for (var i = 0; i < module.ChannelCount; i++)
            {
                try
                {
                    var raw = module.Read4_20mA(i);
                    Resolver.Log.Info($"CH{i}: {raw.Milliamps:N1} mA");
                    var t1 = module.ReadChannelAsConfiguredUnit(i);
                    if (t1 is Temperature temp)
                    {
                        Resolver.Log.Info($"temp{i}: {temp.Fahrenheit:N1}F");
                    }
                }
                catch (Exception ex)
                {
                    Resolver.Log.Error($"ERROR: {ex.Message}");
                }
            }
            Resolver.Log.Info($"---");
            await Task.Delay(1000);
        }
    }

    public async Task Test0_10()
    {
        for (var i = 0; i < 8; i++)
        {
            module.ConfigureChannel(new ChannelConfig
            {
                ChannelNumber = i,
                ChannelType = ConfigurableAnalogInputChannelType.Voltage_0_10,
                UnitType = "Temperature",
                Scale = 3.4725, //0-100F, but scale/offs in C
                Offset = -31.67
            });

            module.ConfigureChannel(new ChannelConfig
            {
                ChannelNumber = i,
                ChannelType = ConfigurableAnalogInputChannelType.ThermistorNtc
            });
        }

        while (true)
        {
            for (var i = 0; i < module.ChannelCount; i++)
            {
                try
                {
                    var raw = module.Read0_10V(i);
                    Resolver.Log.Info($"CH{i}: {raw.Volts:N2} V");
                }
                catch (Exception ex)
                {
                    Resolver.Log.Error($"ERROR: {ex.Message}");
                }
            }
            Resolver.Log.Info($"---");
            await Task.Delay(1000);
        }
    }

    public async Task TestNtc()
    {
        for (var i = 0; i < 8; i++)
        {
            module.ConfigureChannel(new ChannelConfig
            {
                ChannelNumber = i,
                ChannelType = ConfigurableAnalogInputChannelType.ThermistorNtc
            });
        }

        while (true)
        {
            for (var i = 0; i < module.ChannelCount; i++)
            {
                try
                {
                    var raw = module.ReadNtc(i);
                    Resolver.Log.Info($"CH{i}: {raw.Fahrenheit:N2} F");
                }
                catch (Exception ex)
                {
                    Resolver.Log.Error($"ERROR: {ex.Message}");
                }
            }
            Resolver.Log.Info($"---");
            await Task.Delay(1000);
        }
    }

    //<!=SNOP=>
}