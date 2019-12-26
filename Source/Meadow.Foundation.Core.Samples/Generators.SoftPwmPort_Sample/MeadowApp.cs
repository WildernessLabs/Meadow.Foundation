using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Generators;
using Meadow.Hardware;

namespace Generators.SoftPwmPort_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        protected SoftPwmPort softPwmPort;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            IDigitalOutputPort digiOut = Device.CreateDigitalOutputPort(Device.Pins.D00);
            softPwmPort = new SoftPwmPort(digiOut);           
            
            TestSoftPwmPort();
        }

        protected void TestSoftPwmPort()
        {
            Console.WriteLine("TestSoftPwmPort...");

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
