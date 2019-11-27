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
            BrightnessTest(10);
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

        public void BrightnessTest(int loopCount)
        {
            for (int i = 0; i < loopCount; i++) {
                Console.WriteLine("Blue On");
                _bluePwmLed.SetBrightness(1);
                Thread.Sleep(2000);
                _bluePwmLed.SetBrightness(0);
                Console.WriteLine("Blue Off");
                Thread.Sleep(2000);
            }
        }

        public void PulseLeds()
        {
            while (true) {
                //    Console.WriteLine($"State: {state}");
                Console.WriteLine("Pulse Red.");
                this._redPwmLed.StartPulse(5000, lowBrightness: 0.05f);
                Thread.Sleep(5000);
                Console.WriteLine("Stop Red.");
                this._redPwmLed.Stop();

                Console.WriteLine("Pulse Blue.");
                this._bluePwmLed.StartPulse(500, lowBrightness: 0.05f);
                Thread.Sleep(2000);
                Console.WriteLine("Stop Blue.");
                this._bluePwmLed.Stop();

                Console.WriteLine("Pulse Green.");
                this._greenPwmLed.StartPulse(300, lowBrightness: 0.0f);
                Thread.Sleep(2000);
                Console.WriteLine("Stop Green.");
                this._greenPwmLed.Stop();

            }
        }
    }
}