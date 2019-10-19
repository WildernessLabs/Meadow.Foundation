using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Moisture;

namespace Sensors.Moisture.FC28_Sample
{
    /* Driver in development */
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        FC28 fc28;

        public MeadowApp()
        {
            fc28 = new FC28(Device.CreateAnalogInputPort(Device.Pins.A01),
                Device.CreateDigitalOutputPort(Device.Pins.D01));

            TestFC28SensorAsync().Wait();
        }

        async Task TestFC28SensorAsync()
        {
            while (true)
            {
                float moisture = await fc28.Read();
                Console.WriteLine(moisture);
                Thread.Sleep(1000);
            }
        }
    }
}