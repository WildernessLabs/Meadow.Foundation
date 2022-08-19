using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Accelerometers;
using System;
using System.Threading.Tasks;

namespace MeadowApp
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV1>
    {
        public override Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            var bmi270 = new Bmi270(Device.CreateI2cBus());

            return base.Initialize();
        }
    }
}