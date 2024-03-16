using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading.Tasks;

namespace BasicSensors.Atmospheric.Sgp40_Sample;

public class MeadowApp : App<F7FeatherV2>
{
    //<!=SNIP=>

    private Sgp40? sensor;

    public override Task Initialize()
    {
        Resolver.Log.Info("Initializing...");

        sensor = new Sgp40(Device.CreateI2cBus());

        Resolver.Log.Info($"Sensor SN: {sensor.SerialNumber:x6}");

        if (sensor.RunSelfTest())
        {
            Resolver.Log.Info("Self test successful");
        }
        else
        {
            Resolver.Log.Warn("Self test failed");
        }

        var consumer = Sgp40.CreateObserver(
            handler: result =>
            {
                Resolver.Log.Info($"Observer: VOC changed by threshold; new index: {result.New}");
            },
            filter: result =>
            {
                return Math.Abs(result.New - result.Old ?? 0) > 10;
            }
        );
        sensor.Subscribe(consumer);

        sensor.Updated += (sender, result) =>
        {
            Resolver.Log.Info($"  VOC: {result.New}");
        };

        return base.Initialize();
    }

    public override async Task Run()
    {
        await ReadConditions();

        sensor?.StartUpdating(TimeSpan.FromSeconds(1));
    }

    private async Task ReadConditions()
    {
        if (sensor == null) { return; }

        var result = await sensor.Read();
        Resolver.Log.Info("Initial Readings:");
        Resolver.Log.Info($"  Temperature: {result}");
    }

    //<!=SNOP=>
}