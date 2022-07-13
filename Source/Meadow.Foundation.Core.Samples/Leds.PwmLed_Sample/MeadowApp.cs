using Meadow;
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
            Console.WriteLine("Initializing...");

            var onRgbLed = new RgbLed(
                device: Device,
                redPin: Device.Pins.OnboardLedRed,
                greenPin: Device.Pins.OnboardLedGreen,
                bluePin: Device.Pins.OnboardLedBlue);
            onRgbLed.SetColor(RgbLedColors.Red);

            pwmLeds = new List<PwmLed>
            {
                new PwmLed(Device.CreatePwmPort(Device.Pins.D02, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue),
                new PwmLed(Device.CreatePwmPort(Device.Pins.D03, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue),
                new PwmLed(Device.CreatePwmPort(Device.Pins.D04, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue),
                new PwmLed(Device.CreatePwmPort(Device.Pins.D05, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue),
                new PwmLed(Device.CreatePwmPort(Device.Pins.D06, new Frequency(100, Frequency.UnitType.Hertz)), TypicalForwardVoltage.Blue), // This pin throws an exception as PWM Port
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
            Console.WriteLine("TestPwmLeds...");

            pwmLeds[0].Brightness = 2;

            while (true)
            {
                Console.WriteLine("Turning on and off each led for 1 second");
                foreach (var pwmLed in pwmLeds)
                {
                    pwmLed.IsOn = true;
                    await Task.Delay(1000);
                    pwmLed.IsOn = false;
                }

                await Task.Delay(1000);

                Console.WriteLine("Blinking the LED for a bit.");
                foreach (var pwmLed in pwmLeds)
                {
                    pwmLed.StartBlink();
                    await Task.Delay(3000);
                    pwmLed.Stop();
                }

                await Task.Delay(1000);

                Console.WriteLine("Blinking the LED for a bit.");
                foreach (var pwmLed in pwmLeds)
                {
                    pwmLed.StartBlink(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                    await Task.Delay(3000);
                    pwmLed.Stop();
                }

                await Task.Delay(1000);

                Console.WriteLine("Pulsing the LED for a bit.");
                foreach (var pwmLed in pwmLeds)
                {
                    pwmLed.StartPulse();
                    await Task.Delay(1000);
                    pwmLed.Stop();
                }

                await Task.Delay(1000);

                Console.WriteLine("Pulsing the LED for a bit.");
                foreach (var pwmLed in pwmLeds)
                {
                    pwmLed.StartPulse();
                    await Task.Delay(1000);
                    pwmLed.Stop();
                }

                await Task.Delay(1000);

                Console.WriteLine("Set brightness the LED for a bit.");
                foreach (var pwmLed in pwmLeds)
                {
                    pwmLed.Brightness = 0.25f;
                    await Task.Delay(250);
                    pwmLed.Brightness = 0.5f;
                    await Task.Delay(250);
                    pwmLed.Brightness = 0.75f;
                    await Task.Delay(250);
                    pwmLed.Brightness = 1.0f;
                    await Task.Delay(250);
                    pwmLed.Stop();
                }

                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}