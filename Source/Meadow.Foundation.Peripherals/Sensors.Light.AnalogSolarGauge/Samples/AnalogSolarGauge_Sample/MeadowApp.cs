using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>
        AnalogSolarGauge solarGauge;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            solarGauge = new AnalogSolarGauge(Device, Device.Pins.A02, updateInterval: TimeSpan.FromSeconds(1));

            //==== classic .NET Event
            solarGauge.SolarIntensityUpdated += (s, result) => Console.WriteLine($"SolarIntensityUpdated: {result.New * 100:n2}%");
            
            //==== Filterable observer
            var observer = AnalogSolarGauge.CreateObserver(
                handler: result => Console.WriteLine($"Observer filter satisifed, new intensity: {result.New * 100:n2}%"),
                filter: result => {
                    if (result.Old is { } old)
                    {
                        return (Math.Abs(result.New - old) > 0.05); // only notify if change is > 5%
                    }
                    return false;
                });
            solarGauge.Subscribe(observer);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var result = await solarGauge.Read();
            Console.WriteLine($"Solar Intensity: {result * 100:n2}%");

            solarGauge.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}