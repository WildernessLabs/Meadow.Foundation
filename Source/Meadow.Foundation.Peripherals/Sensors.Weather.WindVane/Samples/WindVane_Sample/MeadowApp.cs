using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Weather;
using Meadow.Units;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        RgbPwmLed onboardLed;
        WindVane windVane;

        public MeadowApp()
        {
            Initialize();

            // get initial reading, just to test the API
            Azimuth azi = windVane.Read().Result;
            Console.WriteLine($"Initial azimuth: {azi.Compass16PointCardinalName}");

            // start updating
            windVane.StartUpdating();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            onboardLed = new RgbPwmLed(device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                3.3f, 3.3f, 3.3f,
                Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode);

            // initialize the wind vane driver
            windVane = new WindVane(Device, Device.Pins.A00);
            windVane.Updated += WindVane_Updated;
            windVane.Subscribe(new FilterableChangeObserver<WindVane.WindVaneChangeResult, Azimuth>(
                handler: result => { Console.WriteLine($"Wind Direction: {result.New.Compass16PointCardinalName}"); },
                filter: null
            ));

            Console.WriteLine("Initialization complete.");
        }

        private void WindVane_Updated(object sender, WindVane.WindVaneChangeResult e)
        {
            Console.WriteLine($"Updated event {e.New.DecimalDegrees}");
        }
    }
}