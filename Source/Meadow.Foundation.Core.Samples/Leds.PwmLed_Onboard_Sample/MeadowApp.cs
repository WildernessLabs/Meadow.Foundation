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
            while (true) {
                //    Console.WriteLine($"State: {state}");
                this._redPwmLed.StartPulse(500, lowBrightness: 0.05f);
                Thread.Sleep(500);
                this._redPwmLed.Stop();

                this._bluePwmLed.StartPulse(500, lowBrightness: 0.05f);
                Thread.Sleep(500);
                this._bluePwmLed.Stop();

                this._greenPwmLed.StartPulse(500, lowBrightness: 0.05f);
                Thread.Sleep(500);
                this._greenPwmLed.Stop();

            }
        }
    }
}