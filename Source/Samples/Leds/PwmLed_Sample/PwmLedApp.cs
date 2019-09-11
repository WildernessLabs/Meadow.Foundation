using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PwmLed_Sample
{
    public class PwmLedApp : App<F7Micro, PwmLedApp>
    {
        List<PwmLed> pwmLeds;

        public PwmLedApp()
        {
            Console.WriteLine("Initializing...");

            pwmLeds = new List<PwmLed>();
            pwmLeds.Add(new PwmLed(Device.CreatePwmPort(Device.Pins.D02), TypicalForwardVoltage.Red));
            pwmLeds.Add(new PwmLed(Device.CreatePwmPort(Device.Pins.D03), TypicalForwardVoltage.Green));
            pwmLeds.Add(new PwmLed(Device.CreatePwmPort(Device.Pins.D04), TypicalForwardVoltage.Blue));
            pwmLeds.Add(new PwmLed(Device.CreatePwmPort(Device.Pins.D05), TypicalForwardVoltage.Red));
            pwmLeds.Add(new PwmLed(Device.CreatePwmPort(Device.Pins.D06), TypicalForwardVoltage.Green));
            pwmLeds.Add(new PwmLed(Device.CreatePwmPort(Device.Pins.D07), TypicalForwardVoltage.Blue));
            pwmLeds.Add(new PwmLed(Device.CreatePwmPort(Device.Pins.D08), TypicalForwardVoltage.Red));
            pwmLeds.Add(new PwmLed(Device.CreatePwmPort(Device.Pins.D09), TypicalForwardVoltage.Green));
            pwmLeds.Add(new PwmLed(Device.CreatePwmPort(Device.Pins.D10), TypicalForwardVoltage.Blue));
            pwmLeds.Add(new PwmLed(Device.CreatePwmPort(Device.Pins.D11), TypicalForwardVoltage.Red));
            pwmLeds.Add(new PwmLed(Device.CreatePwmPort(Device.Pins.D12), TypicalForwardVoltage.Green));
            pwmLeds.Add(new PwmLed(Device.CreatePwmPort(Device.Pins.D13), TypicalForwardVoltage.Blue));

            TestPwmLeds();
        }

        protected void TestPwmLeds()
        {
            Console.WriteLine("TestPwmLeds...");

            bool state = true;

            while (true)
            {
                Console.WriteLine("Turning on and off each led for 1 second");
                foreach (var pwmLed in pwmLeds)
                {
                    pwmLed.IsOn = state;
                    Thread.Sleep(500);
                    //pwmLed.IsOn = false;
                }

                state = !state;

                //Console.WriteLine("Blinking the LED for a bit.");
                //foreach (var pwmLed in pwmLeds)
                //{
                //    pwmLed.StartBlink();
                //    Thread.Sleep(3000);
                //    pwmLed.Stop();
                //}

                //Console.WriteLine("Pulsing the LED for a bit.");
                //foreach (var pwmLed in pwmLeds)
                //{
                //    pwmLed.StartPulse();
                //    Thread.Sleep(3000);
                //    pwmLed.Stop();
                //}

                //Console.WriteLine("Set brightness the LED for a bit.");
                //foreach (var pwmLed in pwmLeds)
                //{
                //    pwmLed.SetBrightness(0.25f);
                //    Thread.Sleep(500);
                //    pwmLed.SetBrightness(0.5f);
                //    Thread.Sleep(500);
                //    pwmLed.SetBrightness(0.75f);
                //    Thread.Sleep(500);
                //    pwmLed.SetBrightness(1.0f);
                //    Thread.Sleep(500);
                //    pwmLed.Stop();
                //}
            }
        }
    }
}