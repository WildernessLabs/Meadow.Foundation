using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Adxl362 sensor;

        public MeadowApp()
        {
            Initialize();

            while (true)
            {
                sensor.Update();
                Console.WriteLine("X: " + sensor.X + ", Y: " + sensor.Y + ", Z: " + sensor.Z);
                Thread.Sleep(1000);
            }
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            sensor = new Adxl362(Device, Device.CreateSpiBus(), Device.Pins.D00);
        }
    }
}