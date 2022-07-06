using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using System;
using System.Threading.Tasks;

namespace Leds.PwmLed_Onboard_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        PwmLed redPwmLed;
        PwmLed greenPwmLed;
        PwmLed bluePwmLed;

        public override Task Initialize()
        {
            Console.WriteLine("Initializing...");
            redPwmLed = new PwmLed(Device, Device.Pins.OnboardLedRed, TypicalForwardVoltage.ResistorLimited, CircuitTerminationType.High);
            greenPwmLed = new PwmLed(Device, Device.Pins.OnboardLedGreen, TypicalForwardVoltage.ResistorLimited, CircuitTerminationType.High);
            bluePwmLed = new PwmLed(Device, Device.Pins.OnboardLedBlue, TypicalForwardVoltage.ResistorLimited, CircuitTerminationType.High);
            
            return Task.CompletedTask;
        }

        public override Task Run()
        {
            return PulseLeds();
        }
        
        public async Task BrightnessTest(int loopCount)
        {
            for (int i = 0; i < loopCount; i++) {
                Console.WriteLine("Blue On @ 1.0");
                bluePwmLed.Brightness = 1;
                await Task.Delay(1000);

                Console.WriteLine("Blue at 98.5%");
                bluePwmLed.Brightness = 0.985f;
                await Task.Delay(1000);

                Console.WriteLine("Blue Off");
                bluePwmLed.Brightness = 0;
                await Task.Delay(1000);

                Console.WriteLine("Blue 50%");
                bluePwmLed.Brightness = 0.5f;
                await Task.Delay(1000);
                bluePwmLed.Stop();
            }
        }

        public async Task PulseLeds()
        {
            while (true) 
            {
                Console.WriteLine("Pulse Red.");
                redPwmLed.StartPulse(TimeSpan.FromMilliseconds(500), lowBrightness: 0.05f);
                await Task.Delay(1000);
                Console.WriteLine("Stop Red.");
                redPwmLed.Stop();

                Console.WriteLine("Pulse Blue.");
                bluePwmLed.StartPulse(TimeSpan.FromMilliseconds(500), lowBrightness: 0.05f);
                await Task.Delay(2000);
                Console.WriteLine("Stop Blue.");
                bluePwmLed.Stop();

                Console.WriteLine("Pulse Green.");
                greenPwmLed.StartPulse(TimeSpan.FromMilliseconds(500), lowBrightness: 0.0f);
                await Task.Delay(2000);
                Console.WriteLine("Stop Green.");
                greenPwmLed.Stop();

            }
        }

        //<!=SNOP=>
    }
}