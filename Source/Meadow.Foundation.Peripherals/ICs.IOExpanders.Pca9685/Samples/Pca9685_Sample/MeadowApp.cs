using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System;

namespace ICs.IOExpanders.Pca9685_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        //<!—SNIP—>

        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");
            var i2CBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);

            var pca9685 = new Pca9685(i2CBus, Pca9685.DEFAULT_ADDRESS, 50);
            pca9685.Initialize();
        
              var port0 = pca9685.CreatePwmPort(0, 0.05f);
            var port7 = pca9685.CreatePwmPort(7);

            port0.Start();
            port7.Start();
        }

        //<!—SNOP—>
    }
}
