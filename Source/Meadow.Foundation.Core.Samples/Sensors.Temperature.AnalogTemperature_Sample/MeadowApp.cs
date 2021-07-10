using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using System;
using System.Threading.Tasks;

namespace Sensors.Temperature.AnalogTemperature_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        AnalogTemperature analogTemperature;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our AnalogTemperature sensor
            analogTemperature = new AnalogTemperature (
                device: Device,
                analogPin: Device.Pins.A03,
                sensorType: AnalogTemperature.KnownSensorType.LM35
            );

            //==== IObservable Pattern with an optional notification filter.
            // Example that uses an IObersvable subscription to only be notified
            // when the temperature changes by at least a degree.
            var consumer = AnalogTemperature.CreateObserver(
                handler: result => {
                    Console.WriteLine($"Observer filter satisfied: {result.New.Celsius:N2}C, old: {result.Old?.Celsius:N2}C");
                },
                // only notify if the change is greater than 0.5°C
                filter: result => {
                    if (result.Old is { } old) { //c# 8 pattern match syntax. checks for !null and assigns var.
                        return (result.New - old).Abs().Celsius > 0.5; // returns true if > 0.5°C change.
                    } return false;
                }
                // if you want to always get notified, pass null for the filter:
                //filter: null
            );
            analogTemperature.Subscribe(consumer);

            //==== Classic Events Pattern
            // classical .NET events can also be used:
            analogTemperature.TemperatureUpdated += (sender, result) => {
                Console.WriteLine($"Temp Changed, temp: {result.New.Celsius:N2}C, old: {result.Old?.Celsius:N2}C");
            };

            //==== One-off reading use case/pattern
            ReadTemp().Wait();

            // Spin up the sampling thread so that events are raised and
            // IObservable notifications are sent.
            analogTemperature.StartUpdating(TimeSpan.FromMilliseconds(1000));
        }

        protected async Task ReadTemp()
        {
            var temperature = await analogTemperature.Read();
            Console.WriteLine($"Initial temp: {temperature.Celsius:N2}C");
        }
    }
}