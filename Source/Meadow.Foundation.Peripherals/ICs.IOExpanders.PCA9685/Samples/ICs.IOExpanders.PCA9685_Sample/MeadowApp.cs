using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.ICs;
using Meadow.Foundation.Leds;
using Meadow.Hardware;

namespace ICs.IOExpanders.PCA9685_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        II2cBus _i2CBus;
        PCA9685 _pca9685;

        public MeadowApp()
        {
            Initialize();
            Run();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            _i2CBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);

            _pca9685 = new PCA9685(_i2CBus, 0x40, 50);
            _pca9685.Initialize();
        }

        public void Run()
        {
            var port0 = _pca9685.CreatePwmPort(0, 0.05f);
            var port7 = _pca9685.CreatePwmPort(7);

            port0.Start();
            port7.Start();

        }
    }
}
