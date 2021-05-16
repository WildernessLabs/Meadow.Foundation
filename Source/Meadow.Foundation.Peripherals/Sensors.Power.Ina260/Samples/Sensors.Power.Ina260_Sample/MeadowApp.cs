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
            ina.Updated += (s, v) =>
            {
                Console.WriteLine($"{v.New.Item2}V @ {v.New.Item3}A");
            };

            ina.StartUpdating(TimeSpan.FromSeconds(2));
        }
    }
}