using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Weather;
using Meadow.Units;

namespace MeadowApp
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        SwitchingRainGauge rainGauge;

        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");

            // initialize the rain gauge driver
            rainGauge = new SwitchingRainGauge(Device, Device.Pins.D15);

            //==== Classic event example:
            rainGauge.Updated += (sender, result) => Console.WriteLine($"Updated event {result.New.Millimeters}mm");

            //==== IObservable Pattern
            var observer = SwitchingRainGauge.CreateObserver(
                handler: result => Console.WriteLine($"Rain depth: {result.New.Millimeters}mm"),
                filter: null
            );
            rainGauge.Subscribe(observer);

            // get initial reading, just to test the API - should be 0
            Length rainFall = rainGauge.Read().Result;
            Console.WriteLine($"Initial depth: {rainFall.Millimeters}mm");

            // start the sensor
            rainGauge.StartUpdating();
        }

        //<!—SNOP—>
    }
}