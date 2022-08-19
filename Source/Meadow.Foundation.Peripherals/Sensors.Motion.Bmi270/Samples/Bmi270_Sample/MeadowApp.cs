using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Accelerometers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeadowApp
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV1>
    {
        Bmi270 bmi270;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            bmi270 = new Bmi270(Device.CreateI2cBus());

            return base.Initialize();
        }

        public override Task Run()
        {
            while(true)
            {
                var data = bmi270?.ReadAccelerationData();

                Console.WriteLine(BitConverter.ToString(data));

                Thread.Sleep(2000);
            }

            return base.Run();
        }
    }
}