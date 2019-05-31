using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Moisture;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Capacitive_Sample
{
    public class CapacitiveApp : App<F7Micro, CapacitiveApp>
    {
        Capacitive capacitive;

        public CapacitiveApp()
        {
            capacitive = new Capacitive(Device.CreateAnalogInputPort(Device.Pins.A01));

            TestCapacitiveSensorAsync();
        }

        protected async Task TestCapacitiveSensorAsync()
        {
            while (true)
            {
                float moisture = await capacitive.Read();
                Console.WriteLine(moisture);
                Thread.Sleep(1000);
            }
        }
    }
}
