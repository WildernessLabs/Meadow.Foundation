using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace A02yyuw_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        //<!=SNIP=>

        A02yyuw a02yyuw;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            a02yyuw = new A02yyuw(Device, Device.PlatformOS.GetSerialPortName("COM4"));

            var consumer = A02yyuw.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Observer: Distance changed by threshold; new distance: {result.New.Centimeters:N1}cm, old: {result.Old?.Centimeters:N1}cm");
                },
                filter: result =>
                {
                    if (result.Old is { } old)
                    {
                        return Math.Abs((result.New - old).Centimeters) > 5.0;
                    }
                    return false;
                }
            );
            a02yyuw.Subscribe(consumer);

            a02yyuw.DistanceUpdated += A02yyuw_DistanceUpdated;

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Run...");

            var distance = await a02yyuw.Read();
            Resolver.Log.Info($"Initial distance is: {distance.Centimeters:N1}cm / {distance.Inches:N1}in");

            a02yyuw.StartUpdating(TimeSpan.FromSeconds(2));
        }

        private void A02yyuw_DistanceUpdated(object sender, IChangeResult<Length> e)
        {
            Resolver.Log.Info($"Distance: {e.New.Centimeters:N1}cm / {e.New.Inches:N1}in");
        }

        //<!=SNOP=>
    }
}