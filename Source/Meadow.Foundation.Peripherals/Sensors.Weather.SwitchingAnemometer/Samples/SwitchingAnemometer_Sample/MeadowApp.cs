using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Weather;

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

            onboardLed = new RgbPwmLed(device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                3.3f, 3.3f, 3.3f,
                Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode);

            //==== anemometer
            anemometer = new SwitchingAnemometer(Device, Device.Pins.A01);

            // classic events
            anemometer.WindSpeedUpdated += (object sender, SwitchingAnemometer.AnemometerChangeResult e) =>
            {
                Console.WriteLine($"new speed: {e.New}, old: {e.Old}");
                OutputWindSpeed(e.New);
            };

            //// iobservable
            //anemometer.Subscribe(new FilterableChangeObserver<SwitchingAnemometer.AnemometerChangeResult, float>(
            //    handler: result => {
            //        Console.WriteLine($"new speed: {result.New}, old: {result.Old}");
            //    },
            //    // only notify if it's change more than 0.1kmh:
            //    //filter: result => {
            //    //    Console.WriteLine($"delta: {result.Delta}");
            //    //    return result.Delta > 0.1;
            //    //    }
            //    null
            //));

            anemometer.StartUpdating();

        }

        /// <summary>
        /// Displays the windspeed on the onboard LED as full red @ >= `10km/h`,
        /// blue @ `0km/h`, and a proportional mix, in between those speeds.
        /// </summary>
        /// <param name="windspeed"></param>
        void OutputWindSpeed(float windspeed)
        {
            // `0.0` - `10kmh`
            int r = (int)Map(windspeed, 0f, 10f, 0f, 255f);
            int b = (int)Map(windspeed, 0f, 10f, 255f, 0f);

            //Console.WriteLine($"r: {r}, b: {b}");

            var wspeedColor = Color.FromRgb(r, 0, b);
            onboardLed.SetColor(wspeedColor);
        }

        float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
    }
}