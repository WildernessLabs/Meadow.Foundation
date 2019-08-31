using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using System;
using System.Threading;

namespace PwmLed_Sample
{
    public class PwmLedApp : App<F7Micro, PwmLedApp>
    {        
        readonly PwmLed led2;
        readonly PwmLed led3;
        readonly PwmLed led4;

        public PwmLedApp()
        {
            led2 = new PwmLed(Device.CreatePwmPort(Device.Pins.D10), 3.3f);
            led3 = new PwmLed(Device.CreatePwmPort(Device.Pins.D11), 3.3f);
            led4 = new PwmLed(Device.CreatePwmPort(Device.Pins.D12), 3.3f);
            TestLed(); 
        }

        protected void TestLed()
        {
            while (true)
            {
                for(int i=0; i<=100; i++)
                {
                    float brightness = i / 100f;

                    led2.SetBrightness(brightness);
                    led3.SetBrightness(brightness);
                    led4.SetBrightness(brightness);
                    Thread.Sleep(10);
                }
            }
        }
    }
}