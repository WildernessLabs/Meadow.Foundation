using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using System;
using System.Diagnostics;
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
        sensor.StartUpdating(TimeSpan.FromSeconds(1));

        return base.Initialize();
    }

    private void OnUpdated(object sender, IChangeResult<Meadow.Units.Temperature> e)
    {
        Debug.WriteLine($"Temp: {e.New.Celsius:n1}C  {e.New.Fahrenheit:n1}F");
    }

    //<!=SNOP=>

    public static async Task Main(string[] args)
    {
        await MeadowOS.Start(args);
    }
}