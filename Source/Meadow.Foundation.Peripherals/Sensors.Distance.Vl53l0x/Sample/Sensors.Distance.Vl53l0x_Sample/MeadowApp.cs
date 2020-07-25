using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Hardware;

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
            vL53L0X = new Vl53l0x(i2cBus);
            vL53L0X.Initialize();
        }

        void InitializeWithShutdownPin()
        {
            Console.WriteLine("Initialize hardware...");
            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            var pin = Device.CreateDigitalOutputPort(Device.Pins.D05, true);
            vL53L0X = new Vl53l0x(i2cBus, pin);
            vL53L0X.Initialize();
        }

        void Run()
        {
            Console.WriteLine("Run...");

            var range = vL53L0X.Range();
            Console.WriteLine($"{range} mm");
            
            Thread.Sleep(500);

            vL53L0X.Units = Vl53l0x.UnitType.inches;
            range = vL53L0X.Range();
            Console.WriteLine($"{range} inches");

            Thread.Sleep(500);

            vL53L0X.Units = Vl53l0x.UnitType.mm;

            for (int i = 0; i < 75; i++)
            {
                Thread.Sleep(200);
                range = vL53L0X.Range();
                Console.WriteLine($"{range} mm");
            }

            Console.WriteLine("done...");
        }

        void RunWithShutdownPin()
        {
            Console.WriteLine("Run...");

            var range = vL53L0X.Range();
            Console.WriteLine($"{range} mm");

            Thread.Sleep(500);

            vL53L0X.ShutDown(true);

            //Range will return -1 since the device is off
            range = vL53L0X.Range();
            Console.WriteLine($"{range} mm. IsShutdown { vL53L0X.IsShutdown }");

            //Turn device back on
            vL53L0X.ShutDown(false);

            range = vL53L0X.Range();
            Console.WriteLine($"{range} mm. IsShutdown { vL53L0X.IsShutdown }");

            for (int i = 0; i < 75; i++)
            {
                Thread.Sleep(200);
                range = vL53L0X.Range();
                Console.WriteLine($"{range} mm");
            }

            Console.WriteLine("done...");
        }
    }
}