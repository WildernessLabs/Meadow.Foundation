using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Weather;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        WindVane windVane;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            // initialize the wind vane driver
            windVane = new WindVane(Device.Pins.A00);

            //==== Classic event example:
            windVane.WindAzimuthUpdated += (sender, result) => Resolver.Log.Info($"Updated event {result.New.DecimalDegrees}");

            //==== IObservable Pattern
            var observer = WindVane.CreateObserver(
                handler: result => Resolver.Log.Info($"Wind Direction: {result.New.Compass16PointCardinalName}"),
                filter: null
            );
            windVane.Subscribe(observer);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            // get initial reading, just to test the API
            Azimuth azi = await windVane.Read();
            Resolver.Log.Info($"Initial azimuth: {azi.Compass16PointCardinalName}");

            // start updating
            windVane.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}