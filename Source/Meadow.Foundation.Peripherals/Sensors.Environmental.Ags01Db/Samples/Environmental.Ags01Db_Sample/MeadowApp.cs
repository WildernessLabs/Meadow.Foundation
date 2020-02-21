using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Ags01Db sensor;

        public MeadowApp()
        {
            Initialize();

            Console.WriteLine($"Version: v{sensor.GetVersion()}");

            while (true)
            {
                Console.WriteLine($"VOC gas concentration: {sensor.GetConcentration()}ppm");

                Thread.Sleep(2000);
            }
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            sensor = new Ags01Db(Device.CreateI2cBus());
        }
    }
}