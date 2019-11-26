using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation.Leds;

namespace Leds.PwmLed_Onboard_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        PwmLed _redPwmLed;
        PwmLed _greenPwmLed;
        PwmLed _bluePwmLed;

        public MeadowApp()
        {
            ConfigurePeripherals();
            PulseLeds();
        }

        public void ConfigurePeripherals()
        {
            Console.WriteLine("Creating peripherals...");
            this._redPwmLed = new PwmLed(Device,
                Device.Pins.OnboardLedRed, TypicalForwardVoltage.Red);
            this._greenPwmLed = new PwmLed(Device,
                Device.Pins.OnboardLedGreen, TypicalForwardVoltage.Green);
            this._bluePwmLed = new PwmLed(Device,
                Device.Pins.OnboardLedBlue, TypicalForwardVoltage.Blue);
        }

        public void PulseLeds()
        {
            var state = false;

            while (true) {
                state = !state;

                //    Console.WriteLine($"State: {state}");
                if (state) { this._redPwmLed.StartPulse(500, lowBrightness: 0.05f); }
                else { this._redPwmLed.Stop(); }
                Thread.Sleep(500);

                if (state) { this._bluePwmLed.StartPulse(500, lowBrightness: 0.05f); }
                else { this._bluePwmLed.Stop(); }
                Thread.Sleep(500);

                if (state) { this._greenPwmLed.StartPulse(500, lowBrightness: 0.05f); }
                else { this._greenPwmLed.Stop(); }
                Thread.Sleep(500);

            }
        }
    }
}