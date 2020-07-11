using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System;

namespace ICs.IOExpanders.PCA9685_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Pca9685 pca9685;

        public MeadowApp()
        {
            Initialize();
            Run();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            var i2CBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);

            pca9685 = new Pca9685(i2CBus, 0x40, 50);
            pca9685.Initialize();
        }

        public void Run()
        {
            var port0 = pca9685.CreatePwmPort(0, 0.05f);
            var port7 = pca9685.CreatePwmPort(7);

            port0.Start();
            port7.Start();

        }
    }
}
