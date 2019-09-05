using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Moisture;
using System;
using System.Threading;

namespace Capacitive_Sample
{
    public class CapacitiveApp : App<F7Micro, CapacitiveApp>
    {
        Capacitive capacitive;

        public CapacitiveApp()
        {
            capacitive = new Capacitive(Device.CreateAnalogInputPort(Device.Pins.A00));

            TestCapacitive();
        }

        protected void TestCapacitive()
        {
            while(true)
            {
                Console.WriteLine("Humidity:" + capacitive.Read());
                Thread.Sleep(2000);
            }
        }
    }
}