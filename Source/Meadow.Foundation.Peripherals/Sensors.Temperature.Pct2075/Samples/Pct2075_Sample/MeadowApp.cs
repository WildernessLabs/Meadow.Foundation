using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using System;
using System.Threading.Tasks;

namespace Sensors.Temperature.Pct2075_Sample;

public class MeadowApp : App<F7FeatherV2>
{
    //<!=SNIP=>
    private Pct2075 sensor;

    public override Task Initialize()
    {
        sensor = new Pct2075(Device.CreateI2cBus());

        sensor.Updated += OnUpdated;

        return Task.CompletedTask;
    }

    public override Task Run()
    {
        sensor.StartUpdating(TimeSpan.FromSeconds(1));

        return Task.CompletedTask;
    }

    private void OnUpdated(object sender, IChangeResult<Meadow.Units.Temperature> e)
    {
        Resolver.Log.Info($"Temp: {e.New.Celsius:n1}C  {e.New.Fahrenheit:n1}F");
    }

    //<!=SNOP=>
}