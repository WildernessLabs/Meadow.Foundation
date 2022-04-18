using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation;
using Meadow.Foundation.Leds;

namespace Leds.PwmLed_Onboard_Sample
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        //<!=SNIP=>

        PwmLed redPwmLed;
        PwmLed greenPwmLed;
        PwmLed bluePwmLed;

        public MeadowApp()
        {
            ConfigurePeripherals();
            //BrightnessTest(2);
            PulseLeds();
        }

        public void ConfigurePeripherals()
        {
            Console.WriteLine("Creating peripherals...");
            redPwmLed = new PwmLed(Device, Device.Pins.OnboardLedRed, TypicalForwardVoltage.ResistorLimited, CircuitTerminationType.High);
            greenPwmLed = new PwmLed(Device, Device.Pins.OnboardLedGreen, TypicalForwardVoltage.ResistorLimited, CircuitTerminationType.High);
            bluePwmLed = new PwmLed(Device, Device.Pins.OnboardLedBlue, TypicalForwardVoltage.ResistorLimited, CircuitTerminationType.High);
        }

        public void BrightnessTest(int loopCount)
        {
            for (int i = 0; i < loopCount; i++) {
                Console.WriteLine("Blue On @ 1.0");
                bluePwmLed.SetBrightness(1);
                Thread.Sleep(1000);

                Console.WriteLine("Blue at 98.5%");
                bluePwmLed.SetBrightness(0.985f);
                Thread.Sleep(1000);

                Console.WriteLine("Blue Off");
                bluePwmLed.SetBrightness(0);
                Thread.Sleep(1000);

                Console.WriteLine("Blue 50%");
                bluePwmLed.SetBrightness(0.5f);
                Thread.Sleep(1000);
                bluePwmLed.Stop();
            }
        }

        public void PulseLeds()
        {
            while (true) 
            {
                Console.WriteLine("Pulse Red.");
                redPwmLed.StartPulse(TimeSpan.FromMilliseconds(500), lowBrightness: 0.05f);
                Thread.Sleep(1000);
                Console.WriteLine("Stop Red.");
                redPwmLed.Stop();

                Console.WriteLine("Pulse Blue.");
                bluePwmLed.StartPulse(TimeSpan.FromMilliseconds(500), lowBrightness: 0.05f);
                Thread.Sleep(2000);
                Console.WriteLine("Stop Blue.");
                bluePwmLed.Stop();

                Console.WriteLine("Pulse Green.");
                greenPwmLed.StartPulse(TimeSpan.FromMilliseconds(500), lowBrightness: 0.0f);
                Thread.Sleep(2000);
                Console.WriteLine("Stop Green.");
                greenPwmLed.Stop();

            }
        }

        //<!=SNOP=>
    }
}