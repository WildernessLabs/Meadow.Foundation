using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;
using System;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Alspt19315C sensor;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initialize...");

            // configure our sensor
            sensor = new Alspt19315C(Device.Pins.A03);

            //==== IObservable Pattern with an optional notification filter
            var consumer = Alspt19315C.CreateObserver(
                handler: result => Resolver.Log.Info($"Observer filter satisfied: {result.New.Volts:N2}V, old: {result.Old?.Volts:N2}V"),

                // only notify if the change is greater than 0.5V
                filter: result =>
                {
                    if (result.Old is { } old)
                    { //c# 8 pattern match syntax. checks for !null and assigns var.
                        return (result.New - old).Abs().Volts > 0.5; // returns true if > 0.5V change.
                    }
                    return false;
                }
            );
            sensor.Subscribe(consumer);

            //==== Classic Events Pattern
            sensor.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"Voltage Changed, new: {result.New.Volts:N2}V, old: {result.Old?.Volts:N2}V");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var result = await sensor.Read();
            Resolver.Log.Info($"Initial temp: {result.Volts:N2}V");

            sensor.StartUpdating(TimeSpan.FromMilliseconds(1000));
        }

        //<!=SNOP=>
    }
}