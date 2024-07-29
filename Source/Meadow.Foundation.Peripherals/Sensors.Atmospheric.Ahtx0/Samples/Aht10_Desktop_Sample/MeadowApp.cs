using Meadow;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading.Tasks;

namespace Sensors.Temperature.Aht10_Sample;

public class MeadowApp : App<Windows>
{
    public static async Task Main(string[] args)
    {
        await MeadowOS.Start(args);
    }

    //<!=SNIP=>

    private Ahtx0 sensor;

    public override Task Initialize()
    {
        Resolver.Log.Info("Initialize...");

        // adjust the index to match your hardware configuration
        var ft232 = FtdiExpanderCollection.Devices[0];

        sensor = new Aht10(ft232.CreateI2cBus());

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
        if (sensor == null) { return; }

        var result = await sensor.Read();
        Resolver.Log.Info("Initial Readings:");
        Resolver.Log.Info($"  Temperature: {result.Temperature?.Celsius:F1}°C");
        Resolver.Log.Info($"  Relative Humidity: {result.Humidity:F1}%");

        sensor.StartUpdating(TimeSpan.FromSeconds(1));
    }

    //<!=SNOP=>
}