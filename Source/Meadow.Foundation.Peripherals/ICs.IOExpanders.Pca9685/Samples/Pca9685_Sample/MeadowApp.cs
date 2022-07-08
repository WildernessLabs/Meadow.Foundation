using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Pca9685_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Pca9685 pca9685;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");
            var i2CBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);

            var pca9685 = new Pca9685(i2CBus, new Meadow.Units.Frequency(50, Meadow.Units.Frequency.UnitType.Hertz), (byte)Pca9685.Addresses.Default);
            pca9685.Initialize();

            return base.Initialize();
        }

        public override Task Run()
        {
            var port0 = pca9685.CreatePwmPort(0, 0.05f);
            var port7 = pca9685.CreatePwmPort(7);

            port0.Start();
            port7.Start();

            return base.Run();
        }

        //<!=SNOP=>
    }
}
