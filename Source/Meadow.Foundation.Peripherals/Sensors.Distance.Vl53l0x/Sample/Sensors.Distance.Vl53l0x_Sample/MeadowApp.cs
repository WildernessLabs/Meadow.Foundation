using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sensors.Distance.Vl53l0x_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Vl53l0x vL53L0X;

        public MeadowApp()
        {
            Initialize();
            Run();

            //InitializeWithShutdownPin();
            //RunWithShutdownPin();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            vL53L0X = new Vl53l0x(Device, i2cBus);
            vL53L0X.Initialize();
        }

        void InitializeWithShutdownPin()
        {
            Console.WriteLine("Initialize hardware...");
            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            vL53L0X = new Vl53l0x(Device, i2cBus, Device.Pins.D05);
            vL53L0X.Initialize();
        }

        async Task Run()
        {
            Console.WriteLine("Run...");

            var range = await vL53L0X.Read();
            Console.WriteLine($"{range} mm");
            
            Thread.Sleep(500);

            vL53L0X.Units = Vl53l0x.UnitType.inches;
            range = await vL53L0X.Read();
            Console.WriteLine($"{range} inches");

            Thread.Sleep(500);

            vL53L0X.Units = Vl53l0x.UnitType.mm;

            for (int i = 0; i < 75; i++)
            {
                Thread.Sleep(200);
                range = await vL53L0X.Read();
                Console.WriteLine($"{range} mm");
            }

            Console.WriteLine("done...");
        }

        async Task RunWithShutdownPin()
        {
            Console.WriteLine("Run...");

            var range = vL53L0X.Read();
            Console.WriteLine($"{range} mm");

            Thread.Sleep(500);

            await vL53L0X.ShutDown(true);

            //Range will return -1 since the device is off
            range = vL53L0X.Read();
            Console.WriteLine($"{range} mm. IsShutdown { vL53L0X.IsShutdown }");

            //Turn device back on
            await vL53L0X.ShutDown(false);

            range = vL53L0X.Read();
            Console.WriteLine($"{range} mm. IsShutdown { vL53L0X.IsShutdown }");

            for (int i = 0; i < 75; i++)
            {
                Thread.Sleep(200);
                range = vL53L0X.Read();
                Console.WriteLine($"{range} mm");
            }

            Console.WriteLine("done...");
        }
    }
}