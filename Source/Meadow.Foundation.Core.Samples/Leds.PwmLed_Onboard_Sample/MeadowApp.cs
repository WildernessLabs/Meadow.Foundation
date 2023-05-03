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
            Resolver.Log.Info("Initializing...");
            redPwmLed = new PwmLed(Device.Pins.OnboardLedRed, TypicalForwardVoltage.ResistorLimited, CircuitTerminationType.High);
            greenPwmLed = new PwmLed(Device.Pins.OnboardLedGreen, TypicalForwardVoltage.ResistorLimited, CircuitTerminationType.High);
            bluePwmLed = new PwmLed(Device.Pins.OnboardLedBlue, TypicalForwardVoltage.ResistorLimited, CircuitTerminationType.High);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            return PulseLeds();
        }

        public async Task BrightnessTest(int loopCount)
        {
            for (int i = 0; i < loopCount; i++)
            {
                Resolver.Log.Info("Blue On @ 1.0");
                bluePwmLed.SetBrightness(1);
                await Task.Delay(1000);

                Resolver.Log.Info("Blue at 98.5%");
                bluePwmLed.SetBrightness(0.985f);
                await Task.Delay(1000);

                Resolver.Log.Info("Blue Off");
                bluePwmLed.SetBrightness(0);
                await Task.Delay(1000);

                Resolver.Log.Info("Blue 50%");
                bluePwmLed.SetBrightness(0.5f);
                await Task.Delay(1000);
                await bluePwmLed.StopAnimation();
            }
        }

        public async Task PulseLeds()
        {
            while (true)
            {
                Resolver.Log.Info("Pulse Red.");
                await redPwmLed.StartPulse(TimeSpan.FromMilliseconds(500), lowBrightness: 0.05f);
                await Task.Delay(1000);
                Resolver.Log.Info("Stop Red.");
                await redPwmLed.StopAnimation();

                Resolver.Log.Info("Pulse Blue.");
                await bluePwmLed.StartPulse(TimeSpan.FromMilliseconds(500), lowBrightness: 0.05f);
                await Task.Delay(2000);
                Resolver.Log.Info("Stop Blue.");
                await bluePwmLed.StopAnimation();

                Resolver.Log.Info("Pulse Green.");
                await greenPwmLed.StartPulse(TimeSpan.FromMilliseconds(500), lowBrightness: 0.0f);
                await Task.Delay(2000);
                Resolver.Log.Info("Stop Green.");
                await greenPwmLed.StopAnimation();
            }
        }

        //<!=SNOP=>
    }
}