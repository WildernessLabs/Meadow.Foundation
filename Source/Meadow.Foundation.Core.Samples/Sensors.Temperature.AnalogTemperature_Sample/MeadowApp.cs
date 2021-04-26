using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using System;
using System.Threading.Tasks;

namespace Sensors.Temperature.AnalogTemperature_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        AnalogTemperature analogTemperature;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our AnalogTemperature sensor
            analogTemperature = new AnalogTemperature (
                device: Device,
                analogPin: Device.Pins.A00,
                sensorType: AnalogTemperature.KnownSensorType.LM35
            );

            //==== IObservable Pattern
            // Example that uses an IObersvable subscription to only be notified
            // when the temperature changes by at least a degree.
            var consumer = AnalogTemperature.CreateObserver(
                handler: result => {
                    Console.WriteLine($"Temp changed: {result.New.Celsius:N2}C, old: {result.Old.Celsius:N2}C");
                },
                filter: null
                );
            analogTemperature.Subscribe(consumer);

            //==== Classic Events
            // classical .NET events can also be used:
            analogTemperature.TemperatureUpdated += (object sender, CompositeChangeResult<Meadow.Units.Temperature> e) => {
                Console.WriteLine($"Temp Changed, temp: {e.New.Celsius:N2}C, old: {e.Old.Celsius:N2}C");
            };

            // Get an initial reading.
            ReadTemp().Wait();

            // Spin up the sampling thread so that events are raised and
            // IObservable notifications are sent.
            analogTemperature.StartUpdating();
        }

        protected async Task ReadTemp()
        {
            var temperature = await analogTemperature.Read();
            Console.WriteLine($"Initial temp: {temperature.New.Celsius:N2}C");
        }
    }
}