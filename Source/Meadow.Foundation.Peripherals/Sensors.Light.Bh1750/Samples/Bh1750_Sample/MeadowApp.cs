using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;
using Mode = Meadow.Foundation.Sensors.Light.Bh1750.MeasuringModes;

namespace Sensors.Light.Bh1750_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Bh1750 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var i2c = Device.CreateI2cBus();
            sensor = new Bh1750(
                i2c,
                measuringMode: Mode.ContinuouslyHighResolutionMode, // the various modes take differing amounts of time.
                lightTransmittance: 1 // lower this to increase sensitivity, for instance, if it's behind a semi opaque window
                ); 

            // Example that uses an IObservable subscription to only be notified when the filter is satisfied
            var consumer = Bh1750.CreateObserver(
                handler: result => Resolver.Log.Info($"Observer: filter satisifed: {result.New.Lux:N2}Lux, old: {result.Old?.Lux:N2}Lux"),
                
                // only notify if the visible light changes by 100 lux (put your hand over the sensor to trigger)
                filter: result => {
                    if (result.Old is { } old) { //c# 8 pattern match syntax. checks for !null and assigns var.
                        // returns true if > 100lux change
                        return ((result.New - old).Abs().Lux > 100);
                    }
                    return false;
                });
            sensor.Subscribe(consumer);

            // classical .NET events can also be used:
            sensor.Updated += (sender, result) => Resolver.Log.Info($"Light: {result.New.Lux:N2}Lux");

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var result = await sensor.Read();
            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info($" Light: {result.Lux:N2}Lux");

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}