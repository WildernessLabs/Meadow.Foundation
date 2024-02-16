﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leds.PwmLed_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        List<PwmLed> pwmLeds;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            var onRgbLed = new RgbLed(
                redPin: Device.Pins.OnboardLedRed,
                greenPin: Device.Pins.OnboardLedGreen,
                bluePin: Device.Pins.OnboardLedBlue);
            onRgbLed.SetColor(RgbLedColors.Red);

            pwmLeds = new List<PwmLed>
            {
                new PwmLed(Device.CreatePwmPort(Device.Pins.D02, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue),
                new PwmLed(Device.CreatePwmPort(Device.Pins.D03, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue),
                new PwmLed(Device.CreatePwmPort(Device.Pins.D04, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue),
                //new PwmLed(Device.CreatePwmPort(Device.Pins.D05, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue),
                //new PwmLed(Device.CreatePwmPort(Device.Pins.D06, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue), // This pin throws an exception as PWM Port
                new PwmLed(Device.CreatePwmPort(Device.Pins.D07, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue),
                new PwmLed(Device.CreatePwmPort(Device.Pins.D08, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue),
                new PwmLed(Device.CreatePwmPort(Device.Pins.D09, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue),
                new PwmLed(Device.CreatePwmPort(Device.Pins.D10, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue),
                new PwmLed(Device.CreatePwmPort(Device.Pins.D11, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue),
                new PwmLed(Device.CreatePwmPort(Device.Pins.D12, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue),
                new PwmLed(Device.CreatePwmPort(Device.Pins.D13, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue)
            };

            onRgbLed.SetColor(RgbLedColors.Green);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info("TestPwmLeds...");

            while (true)
            {
                Resolver.Log.Info("Turning on and off each led for 1 second");
                foreach (var pwmLed in pwmLeds)
                {
                    pwmLed.IsOn = true;
                    await Task.Delay(1000);
                    pwmLed.IsOn = false;
                }

                await Task.Delay(1000);

                Resolver.Log.Info("Blinking each LED (on 500ms / off 500ms)");
                foreach (var pwmLed in pwmLeds)
                {
                    await pwmLed.StartBlink();
                    await Task.Delay(3000);
                    await pwmLed.StopAnimation();
                }

                await Task.Delay(1000);

                Resolver.Log.Info("Blinking each LED (on 1s / off 1s)");
                foreach (var pwmLed in pwmLeds)
                {
                    await pwmLed.StartBlink(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                    await Task.Delay(3000);
                    await pwmLed.StopAnimation();
                }

                await Task.Delay(1000);

                Resolver.Log.Info("Pulsing each LED (600ms pulse duration)");
                foreach (var pwmLed in pwmLeds)
                {
                    await pwmLed.StartPulse();
                    await Task.Delay(1000);
                    await pwmLed.StopAnimation();
                    pwmLed.IsOn = false;
                }

                await Task.Delay(1000);

                Resolver.Log.Info("Set brightness the LED for a bit.");
                foreach (var pwmLed in pwmLeds)
                {
                    pwmLed.SetBrightness(0.25f);
                    await Task.Delay(250);
                    pwmLed.SetBrightness(0.5f);
                    await Task.Delay(250);
                    pwmLed.SetBrightness(0.75f);
                    await Task.Delay(250);
                    pwmLed.SetBrightness(1.0f);
                    await Task.Delay(250);
                    pwmLed.IsOn = false;
                }

                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}