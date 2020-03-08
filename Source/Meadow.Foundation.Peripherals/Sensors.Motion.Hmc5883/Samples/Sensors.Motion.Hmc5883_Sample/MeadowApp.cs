using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Hmc5883 sensor;

        public MeadowApp()
        {
            InitHardware();

            while(true)
            {
                Console.WriteLine($"Read data");
              //  sensor.GetDirection();
                Thread.Sleep(50);

               
            }
        }

        public void InitHardware()
        {
            Console.WriteLine("Initialize...");

            var bus = Device.CreateI2cBus();

            sensor = new Hmc5883(bus); 
        }
    }
}