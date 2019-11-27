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
                Device.Pins.OnboardLedRed, TypicalForwardVoltage.ResistorLimited, inverted:true);
            this._greenPwmLed = new PwmLed(Device,
                Device.Pins.OnboardLedGreen, TypicalForwardVoltage.ResistorLimited, inverted: true);
            this._bluePwmLed = new PwmLed(Device,
                Device.Pins.OnboardLedBlue, TypicalForwardVoltage.ResistorLimited, inverted: true);
        }

        public void PulseLeds()
        {
            while (true) {
                //    Console.WriteLine($"State: {state}");
                this._redPwmLed.StartPulse(10000, lowBrightness: 0.05f);
                Thread.Sleep(10000);
                this._redPwmLed.Stop();

                this._bluePwmLed.StartPulse(500, lowBrightness: 0.05f);
                Thread.Sleep(2000);
                this._bluePwmLed.Stop();

                this._greenPwmLed.StartPulse(300, lowBrightness: 0.0f);
                Thread.Sleep(2000);
                this._greenPwmLed.Stop();

            }
        }
    }
}