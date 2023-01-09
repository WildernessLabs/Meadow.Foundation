using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using System;
using System.Threading.Tasks;

namespace Sensors.Temperature.AnalogTemperature_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>
        
        AnalogTemperature analogTemperature;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            // configure our AnalogTemperature sensor
            analogTemperature = new AnalogTemperature (
                device: Device,
                analogPin: Device.Pins.A03,
                sensorType: AnalogTemperature.KnownSensorType.LM35
            );

            //==== IObservable Pattern with an optional notification filter.
            var consumer = AnalogTemperature.CreateObserver(
                handler: result => Resolver.Log.Info($"Observer filter satisfied: {result.New.Celsius:N2}C, old: {result.Old?.Celsius:N2}C"),

                // only notify if the change is greater than 0.5°C
                filter: result => {
                    if (result.Old is { } old) 
                    {   //c# 8 pattern match syntax. checks for !null and assigns var.
                        return (result.New - old).Abs().Celsius > 0.5; // returns true if > 0.5°C change.
                    }
                    return false;
                }
                // if you want to always get notified, pass null for the filter:
                //filter: null
            );
            analogTemperature.Subscribe(consumer);

            // classical .NET events can also be used:
            analogTemperature.TemperatureUpdated += (sender, result) => {
                Resolver.Log.Info($"Temp Changed, temp: {result.New.Celsius:N2}C, old: {result.Old?.Celsius:N2}C");
            };

            //==== One-off reading use case/pattern
            ReadTemp().Wait();

            // Spin up the sampling thread so that events are raised and IObservable notifications are sent.
            analogTemperature.StartUpdating(TimeSpan.FromMilliseconds(1000));

            return Task.CompletedTask;
        }

        protected async Task ReadTemp()
        {
            var temperature = await analogTemperature.Read();
            Resolver.Log.Info($"Initial temp: {temperature.Celsius:N2}C");
        }

        //<!=SNOP=>
    }
}