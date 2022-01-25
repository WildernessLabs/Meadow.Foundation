using Meadow.Devices;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Sound
{
    //WIP
   public class Ky038
    {
        protected IAnalogInputPort analogPort;
        protected IDigitalInputPort digitalInputPort;

        public Ky038(IMeadowDevice device, IPin A0, IPin D0) : 
            this (device.CreateAnalogInputPort(A0, 5, TimeSpan.FromMilliseconds(50), new Units.Voltage(3.3)), 
                device.CreateDigitalInputPort(D0))
        {
        }

        public Ky038(IAnalogInputPort analogPort, IDigitalInputPort digitalInputPort)
        {
            this.analogPort = analogPort;
            this.digitalInputPort = digitalInputPort;

            digitalInputPort.Changed += DigitalInputPort_Changed;

            analogPort.StartUpdating(TimeSpan.FromSeconds(1));

            while (true)
            {
                Console.WriteLine($"Analog: {analogPort.Voltage}");
                Thread.Sleep(250);
            }
        }

        private void DigitalInputPort_Changed(object sender, DigitalPortResult e)
        {
           
        }
    }
}