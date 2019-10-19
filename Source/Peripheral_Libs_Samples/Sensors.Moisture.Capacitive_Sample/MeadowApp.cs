using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Moisture;

namespace Sensors.Moisture.Capacitive_Sample
{
    /* Driver in development */
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Capacitive capacitive;

        public MeadowApp()
        {
            capacitive = new Capacitive(Device.CreateAnalogInputPort(Device.Pins.A01));

            TestCapacitiveSensorAsync();
        }

        async Task TestCapacitiveSensorAsync()
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
