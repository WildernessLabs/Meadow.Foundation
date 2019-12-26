using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation;
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
            //BrightnessTest(2);
            PulseLeds();
        }

        public void ConfigurePeripherals()
        {
            Console.WriteLine("Creating peripherals...");
            this._redPwmLed = new PwmLed(Device,
                Device.Pins.OnboardLedRed, TypicalForwardVoltage.ResistorLimited, CircuitTerminationType.High);
            this._greenPwmLed = new PwmLed(Device,
                Device.Pins.OnboardLedGreen, TypicalForwardVoltage.ResistorLimited, CircuitTerminationType.High);
            this._bluePwmLed = new PwmLed(Device,
                Device.Pins.OnboardLedBlue, TypicalForwardVoltage.ResistorLimited, CircuitTerminationType.High);
        }

        public void BrightnessTest(int loopCount)
        {
            for (int i = 0; i < loopCount; i++) {
                Console.WriteLine("Blue On @ 1.0");
                _bluePwmLed.SetBrightness(1);
                Thread.Sleep(1000);
                Console.WriteLine("Blue at 98.5%");
                _bluePwmLed.SetBrightness(0.985f);
                Thread.Sleep(1000);
                Console.WriteLine("Blue Off");
                _bluePwmLed.SetBrightness(0);
                Thread.Sleep(1000);
                Console.WriteLine("Blue 50%");
                _bluePwmLed.SetBrightness(0.5f);
                Thread.Sleep(1000);
                _bluePwmLed.Stop();
            }
        }

        public void PulseLeds()
        {
            while (true) {
                //    Console.WriteLine($"State: {state}");
                Console.WriteLine("Pulse Red.");
                this._redPwmLed.StartPulse(500, lowBrightness: 0.05f);
                Thread.Sleep(1000);
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