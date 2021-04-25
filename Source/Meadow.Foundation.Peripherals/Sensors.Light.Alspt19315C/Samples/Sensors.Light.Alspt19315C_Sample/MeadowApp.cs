using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Alspt19315C sensor;

        public MeadowApp()
        {
            Init();

            Task.Run(async () =>
            {
                while(true)
                {
                    Console.WriteLine($"Voltage: {sensor.Read()}V");
                    await Task.Delay(500);
                }
            });
        }

        public void Init()
        {
            Console.WriteLine("Init...");

            sensor = new Alspt19315C(Device, Device.Pins.A01);
        }
    }
}