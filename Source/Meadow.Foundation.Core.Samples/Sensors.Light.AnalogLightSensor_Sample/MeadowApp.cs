using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;
using System;
using System.Threading.Tasks;

namespace Sensors.Light.AnalogLightSensor_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        AnalogLightSensor analogLightSensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            // configure our AnalogLightSensor sensor
            analogLightSensor = new AnalogLightSensor(
                analogPin: Device.Pins.A03);

            //==== IObservable Pattern with an optional notification filter.
            var consumer = AnalogLightSensor.CreateObserver(
                handler: result => Resolver.Log.Info($"Observer filter satisfied: {result.New.Lux:N2} lux, old: {result.Old.Value.Lux:N2} lux"),

                // only notify if the change is greater than 0.5
                filter: result =>
                {
                    if (result.Old is { } old)
                    {
                        return (result.New - old).Abs().Lux > 0.5; // returns true if > 0.5  change.
                    }
                    return false;
                }
                // if you want to always get notified, pass null for the filter:
                //filter: null
            );
            analogLightSensor.Subscribe(consumer);

            // classical .NET events can also be used:
            analogLightSensor.LuminosityUpdated += (sender, result) =>
                Resolver.Log.Info($"Lux changed: {result.New.Lux:N2} lux, old: {result.Old?.Lux:N2} lux");

            //==== One-off reading use case/pattern
            ReadIlluminance().Wait();

            // Spin up the sampling thread so that events are raised and IObservable notifications are sent.
            analogLightSensor.StartUpdating(TimeSpan.FromMilliseconds(1000));

            return Task.CompletedTask;
        }

        protected async Task ReadIlluminance()
        {
            var illuminance = await analogLightSensor.Read();
            Resolver.Log.Info($"Initial lux: {illuminance.Lux:N2} lux");
        }

        //<!=SNOP=>
    }
}