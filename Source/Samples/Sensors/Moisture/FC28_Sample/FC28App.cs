using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Moisture;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FC28_Sample
{
    public class FC28App : App<F7Micro, FC28App>
    {
        FC28 fc28;

        public FC28App()
        {
            fc28 = new FC28(Device.CreateAnalogInputPort(Device.Pins.A01),
                Device.CreateDigitalOutputPort(Device.Pins.D01));

            TestFC28SensorAsync();
        }

        protected async Task TestFC28SensorAsync()
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