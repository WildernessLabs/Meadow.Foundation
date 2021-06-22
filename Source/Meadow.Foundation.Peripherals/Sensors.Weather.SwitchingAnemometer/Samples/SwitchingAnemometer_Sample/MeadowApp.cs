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
        SwitchingAnemometer anemometer;

        public MeadowApp()
        {
            Initialize();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            //==== onboard LED
            onboardLed = new RgbPwmLed(device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                3.3f, 3.3f, 3.3f,
                Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode);

            //==== create the anemometer
            anemometer = new SwitchingAnemometer(Device, Device.Pins.A01);

            //==== classic events example
            anemometer.WindSpeedUpdated += (sender, result) =>
            {
                Console.WriteLine($"new speed: {result.New.KilometersPerHour:n1}kmh, old: {result.Old?.KilometersPerHour:n1}kmh");
                OutputWindSpeed(result.New);
            };

            //==== IObservable example
            var observer = SwitchingAnemometer.CreateObserver(
                handler: result => {
                    Console.WriteLine($"new speed (from observer): {result.New.KilometersPerHour:n1}kmh, old: {result.Old?.KilometersPerHour:n1}kmh");
                },
                // only notify if it's change more than 0.1kmh:
                //filter: result => {
                //    Console.WriteLine($"delta: {result.Delta}");
                //    return result.Delta > 0.1;
                //    }
                null
                );
            anemometer.Subscribe(observer);

            // start raising updates
            anemometer.StartUpdating();

            Console.WriteLine("Hardware initialized.");
        }

        /// <summary>
        /// Displays the windspeed on the onboard LED as full red @ >= `10km/h`,
        /// blue @ `0km/h`, and a proportional mix, in between those speeds.
        /// </summary>
        /// <param name="windspeed"></param>
        void OutputWindSpeed(Speed windspeed)
        {
            // `0.0` - `10kmh`
            int r = (int)windspeed.KilometersPerHour.Map(0f, 10f, 0f, 255f);
            int b = (int)windspeed.KilometersPerHour.Map(0f, 10f, 255f, 0f);

            //Console.WriteLine($"r: {r}, b: {b}");

            var wspeedColor = Color.FromRgb(r, 0, b);
            onboardLed.SetColor(wspeedColor);
        }
    }
}