using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Weather;
using Meadow.Units;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        WindVane windVane;

        public MeadowApp()
        {
            Initialize();

            // get initial reading, just to test the API
            Azimuth azi = windVane.Read().Result;
            Console.WriteLine($"Initial azimuth: {azi.Compass16PointCardinalName}");

            // start updating
            windVane.StartUpdating(TimeSpan.FromSeconds(1));
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            // initialize the wind vane driver
            windVane = new WindVane(Device, Device.Pins.A00);

            //==== Classic event example:
            windVane.Updated += (sender, result) => {
                Console.WriteLine($"Updated event {result.New.DecimalDegrees}");
            };

            //==== IObservable Pattern
            var observer = WindVane.CreateObserver(
                handler: result => { Console.WriteLine($"Wind Direction: {result.New.Compass16PointCardinalName}"); },
                filter: null
            );
            windVane.Subscribe(observer);


            Console.WriteLine("Initialization complete.");
        }

    }
}