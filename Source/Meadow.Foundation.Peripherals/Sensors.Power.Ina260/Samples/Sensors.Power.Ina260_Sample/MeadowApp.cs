using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Power;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        public MeadowApp()
        {
            InitHardware();
        }

        public void InitHardware()
        {
            Console.WriteLine("Initialize...");
            var bus = Device.CreateI2cBus();
            var ina = new Ina260(bus);

            Console.WriteLine($"-- INA260 Sample App ---");
            Console.WriteLine($"Manufacturer: {ina.ManufacturerID}");
            Console.WriteLine($"Die: {ina.DieID}");

            while (true)
            {
                Console.WriteLine($"{ina.Voltage}V @ {ina.Current}A");
                Thread.Sleep(1000);
            }
        }
    }
}