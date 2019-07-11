using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Hardware;

namespace PwmLed_Sample
{
    public class PwmLedApp : App<F7Micro, PwmLedApp>
    {
        readonly IPwmPort pwm; 
        readonly PwmLed led;

        public PwmLedApp()
        {
            pwm = Device.CreatePwmPort(Device.Pins.D05);

            led = new PwmLed(pwm, 3.3f);
            TestLed();
        }

        protected void TestLed()
        {
            while (true)
            {
                Console.WriteLine("Turning the LED on for 1 second");
                led.IsOn = true;
                Thread.Sleep(1000);
                led.IsOn = false;

                Console.WriteLine("Blinking the LED for 1 second");
                led.StartBlink();
                Thread.Sleep(1000);
                led.Stop();

                Console.WriteLine("Pulsing the LED for 2 seconds");
                led.StartPulse();
                Thread.Sleep(2000);
                led.Stop();
            }
        }
    }
}
