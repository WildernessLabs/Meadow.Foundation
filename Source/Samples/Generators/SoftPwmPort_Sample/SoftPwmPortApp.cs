using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Generators;
using Meadow.Hardware;

namespace SoftPwmPort_Sample
{
    public class SoftPwmPortApp : App<F7Micro, SoftPwmPortApp>
    {        
        SoftPwmPort softPwmPort;

        public SoftPwmPortApp()
        {
            IDigitalOutputPort digiOut = Device.CreateDigitalOutputPort(Device.Pins.D00);
            Console.WriteLine("digital out created");
            softPwmPort = new SoftPwmPort(digiOut);
            Console.WriteLine("SoftPwmPort created");
            StartPulsing();
        }

        public void StartPulsing()
        {
            softPwmPort.Start();

            while (true)
            {
                softPwmPort.DutyCycle = 0.2f;
                Thread.Sleep(500);
                softPwmPort.DutyCycle = 0.5f;
                Thread.Sleep(500);
                softPwmPort.DutyCycle = 0.75f;
                Thread.Sleep(500);
                softPwmPort.DutyCycle = 1f;
                Thread.Sleep(500);
            }
        }
    }
}
