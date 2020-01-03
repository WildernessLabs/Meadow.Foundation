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
        Temt6000 sensor;

        public MeadowApp()
        {
            Init();

            Task.Run(async () =>
            {
                while(true)
                {
                    Console.WriteLine($"Voltage: {await sensor.GetVoltage()}");
                    await Task.Delay(500);
                }
            });
        }

        public void Init()
        {
            Console.WriteLine("Init...");

            sensor = new Temt6000(Device, Device.Pins.A03);
        }
    }
}