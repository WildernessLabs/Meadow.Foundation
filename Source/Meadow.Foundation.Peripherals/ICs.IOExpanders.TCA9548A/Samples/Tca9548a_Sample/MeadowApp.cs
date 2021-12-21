using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System;
using System.Threading;

namespace ICs.IOExpanders.Tca9685_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");

            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.Standard);
            var tca9548a = new Tca9548a(i2cBus, 0x70);
            var mcp0 = new Mcp23x08(tca9548a.Bus0);
            var mcp1 = new Mcp23x08(tca9548a.Bus1);
          
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

        //<!—SNOP—>
    }
}
