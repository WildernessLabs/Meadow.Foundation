using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Qmc5883 sensor;

        public MeadowApp()
        {
            InitHardware();

            while (true)
            {
                Console.WriteLine("Read data");
                Thread.Sleep(500);
                sensor.GetDirection();
                Thread.Sleep(500);
            }
        }

        public void InitHardware()
        {
            Console.WriteLine("Initialize...");

            var bus = Device.CreateI2cBus();

            sensor = new Qmc5883(bus);
        }
    }
}