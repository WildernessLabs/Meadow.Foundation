using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Generators;
using Meadow.Foundation.Leds;
using Meadow.Hardware;

namespace PwmLed_Sample
{
    public class PwmLedApp : AppBase<F7Micro, PwmLedApp>
    {
        IDigitalOutputPort port;
        SoftPwmPort pwm; // TODO: get rid of this when we get hadware PWM working.
        PwmLed led;

        public PwmLedApp()
        {
            port = Device.CreateDigitalOutputPort(Device.Pins.D00);
            pwm = new SoftPwmPort(port);
            led = new PwmLed(pwm, 3.3f);
            this.TestLed();
        }

        protected void TestLed()
        {
            while (true) {
                Console.WriteLine("Turning the LED on for a bit.");
                led.IsOn = true;
                Thread.Sleep(1000);
                led.IsOn = false;

                Console.WriteLine("Blinking the LED for a bit.");
                led.StartBlink();
                Thread.Sleep(1000);
                led.Stop();

                Console.WriteLine("Pulsing the LED for a bit.");
                led.StartPulse();
                Thread.Sleep(5000);
                led.Stop();

            }
        }
    }
}
