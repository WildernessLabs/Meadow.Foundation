using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System;
using System.Threading;

namespace ICs.IOExpanders.Tca9685_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Tca9548a tca9548a;
        II2cBus i2cBus;
        Mcp23x08 mcp0;
        Mcp23x08 mcp1;

        public MeadowApp()
        {
            Initialize();
            Run();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            i2cBus = Device.CreateI2cBus(I2cBusSpeed.Standard);
            tca9548a = new Tca9548a(i2cBus, 0x70);
            mcp0 = new Mcp23x08(tca9548a.Bus0);
            mcp1 = new Mcp23x08(tca9548a.Bus1);
        }

        public void Run()
        {
            var bus0Port0 = mcp0.CreateDigitalOutputPort(mcp0.Pins.GP0);
            var bus1Port0 = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP0);
            while (true)
            {
                bus0Port0.State = true;
                bus1Port0.State = false;
                Thread.Sleep(1000);
                bus0Port0.State = false;
                bus1Port0.State = true;
                Thread.Sleep(1000);
            }
        }
    }
}
