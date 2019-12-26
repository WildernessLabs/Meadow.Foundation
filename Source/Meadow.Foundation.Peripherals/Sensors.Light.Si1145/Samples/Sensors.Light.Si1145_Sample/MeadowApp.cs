using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Si1145 sensor;
    
        public MeadowApp()
        {
            Initialize();

            while (true)
            {
                Console.WriteLine($"IR: {sensor.GetIfrared()}");
                Console.WriteLine($"UV: {sensor.GetUltraViolet()}");
                Console.WriteLine($"VI: {sensor.GetVisible()}");
                Console.WriteLine($"PX: {sensor.GetProximity()}");

                Thread.Sleep(5000);
            }
        }

        public void Initialize()
        {
            Console.WriteLine("Init...");

            sensor = new Si1145(Device.CreateI2cBus());
        }
    }
}