using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading.Tasks;

namespace Sensors.Temperature.Aht10_Sample;

public class FeatherV1App : MeadowApp<F7FeatherV1> { }
public class FeatherV2App : MeadowApp<F7FeatherV2> { }
public class CoreComputApp : MeadowApp<F7CoreComputeV2> { }

public class MeadowApp<T> : App<T>
    where T : F7MicroBase
{
    //<!=SNIP=>

    private Ahtx0 sensor;

    public override Task Initialize()
    {
        Resolver.Log.Info("Initialize...");

        sensor = new Aht10(Device.CreateI2cBus());

        var consumer = Aht10.CreateObserver(
            handler: (result) =>
            {
                Resolver.Log.Info($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2}C, old: {result.Old?.Temperature?.Celsius:N2}C");
            },
            filter: null
        );
        sensor.Subscribe(consumer);

        (sensor as ITemperatureSensor).Updated += (sender, e) =>
        {
            Resolver.Log.Info($"Temperature Updated: {e.New.Celsius:n2}C");
        };
        return Task.CompletedTask;
    }

    public override async Task Run()
    {
        Resolver.Log.Info("Run...");

        if (sensor == null) { return; }

        var result = await sensor.Read();
        Resolver.Log.Info("Initial Readings:");
        Resolver.Log.Info($"  Temperature: {result.Temperature?.Celsius:F1}°C");
        Resolver.Log.Info($"  Relative Humidity: {result.Humidity:F1}%");

        sensor.StartUpdating(TimeSpan.FromSeconds(1));
    }

    //<!=SNOP=>
}