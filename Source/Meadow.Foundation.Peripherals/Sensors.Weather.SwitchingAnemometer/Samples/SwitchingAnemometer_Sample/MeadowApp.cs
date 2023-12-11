using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Weather;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        SwitchingAnemometer anemometer;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            anemometer = new SwitchingAnemometer(Device.Pins.A01);

            //==== classic events example
            anemometer.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"new speed: {result.New.KilometersPerHour:n1}kmh, old: {result.Old?.KilometersPerHour:n1}kmh");
            };

            //==== IObservable example
            var observer = SwitchingAnemometer.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"new speed (from observer): {result.New.KilometersPerHour:n1}kmh, old: {result.Old?.KilometersPerHour:n1}kmh");
                },
                null
                );
            anemometer.Subscribe(observer);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            // start raising updates
            anemometer.StartUpdating();
            Resolver.Log.Info("Hardware initialized.");

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}