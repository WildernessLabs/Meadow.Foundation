using System;
using System.Diagnostics;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Generators;
using Meadow.Hardware;

namespace SoftPwmPort_Sample
{
    public class SoftPwmPortApp : AppBase<F7Micro, SoftPwmPortApp>
    {
        IDigitalOutputPort digiOut;
        SoftPwmPort softPwmPort;

        public SoftPwmPortApp()
        {
            digiOut = Device.CreateDigitalOutputPort(Device.Pins.D00);
            Debug.WriteLine("digital out created");
            softPwmPort = new SoftPwmPort(digiOut);
            Debug.WriteLine("SoftPwmPort created");
            StartPulsing();
        }

        public void StartPulsing()
        {
            softPwmPort.Start();
            while (true) {
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
