using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Flow;
using Meadow.Peripherals.Leds;
using System;
using System.Threading.Tasks;

namespace MeadowApp;

public class MeadowApp : App<F7FeatherV2>
{
    private SwissflowSF800 _sf800 = default!;

    public override Task Initialize()
    {
        Resolver.Log.Info("Initialize...");

        _sf800 = new SwissflowSF800(Device.Pins.D05);
        _sf800.FlowStarted += (s, e) => Resolver.Log.Info("Flow started");
        _sf800.FlowStopped += (s, e) => Resolver.Log.Info($"Flow stopped: {e.Volume.Liters} Liters");
        return Task.CompletedTask;
    }

    public override Task Run()
    {
        return Task.CompletedTask;
    }
}