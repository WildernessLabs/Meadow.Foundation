﻿using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Weather;
using Meadow.Peripherals.Leds;
using Meadow.Units;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        RgbPwmLed onboardLed;
        SwitchingAnemometer anemometer;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            //==== onboard LED
            onboardLed = new RgbPwmLed(device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                CommonType.CommonAnode);

            //==== create the anemometer
            anemometer = new SwitchingAnemometer(Device, Device.Pins.A01);

            //==== classic events example
            anemometer.WindSpeedUpdated += (sender, result) =>
            {
                Resolver.Log.Info($"new speed: {result.New.KilometersPerHour:n1}kmh, old: {result.Old?.KilometersPerHour:n1}kmh");
                OutputWindSpeed(result.New);
            };

            //==== IObservable example
            var observer = SwitchingAnemometer.CreateObserver(
                handler: result => {
                    Resolver.Log.Info($"new speed (from observer): {result.New.KilometersPerHour:n1}kmh, old: {result.Old?.KilometersPerHour:n1}kmh");
                },
                null
                );
            anemometer.Subscribe(observer);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            // start raising updates
            anemometer.StartUpdating();
            Resolver.Log.Info("Hardware initialized.");
            
            return Task.CompletedTask;
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

            var wspeedColor = Color.FromRgb(r, 0, b);
            onboardLed.SetColor(wspeedColor);
        }
        //<!=SNOP=>
    }
}