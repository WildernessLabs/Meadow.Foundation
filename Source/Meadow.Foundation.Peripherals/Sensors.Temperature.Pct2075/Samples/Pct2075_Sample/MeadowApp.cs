using Meadow;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Temperature;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sensors.Temperature.Pct2075_Sample;

public class MeadowApp : App<Windows>
{
    //<!=SNIP=>
    private Ft232h _expander;
    private Pct2075 _sensor;

    public override Task Initialize()
    {
        _expander = new Ft232h(false);
        _sensor = new Pct2075(_expander.CreateI2cBus());

        _sensor.Updated += OnUpdated;
        _sensor.StartUpdating(TimeSpan.FromSeconds(1));

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