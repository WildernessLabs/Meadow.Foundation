using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Me007ys_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Me007ys me007ys;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            me007ys = new Me007ys(Device, Device.PlatformOS.GetSerialPortName("COM1"));

            var consumer = Me007ys.CreateObserver(
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
            me007ys.Subscribe(consumer);

            me007ys.DistanceUpdated += Me007y_DistanceUpdated;

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Run...");

            var distance = await me007ys.Read();
            Resolver.Log.Info($"Initial distance is: {distance.Centimeters:N1}cm / {distance.Inches:N1}in");

            me007ys.StartUpdating(TimeSpan.FromSeconds(2));
        }

        private void Me007y_DistanceUpdated(object sender, IChangeResult<Length> e)
        {
            Resolver.Log.Info($"Distance: {e.New.Centimeters:N1}cm / {e.New.Inches:N1}in");
        }

        //<!=SNOP=>
    }
}