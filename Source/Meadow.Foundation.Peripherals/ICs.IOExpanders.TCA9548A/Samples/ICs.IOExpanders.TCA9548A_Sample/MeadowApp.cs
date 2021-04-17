using Meadow;
using Meadow.Foundation;
using Meadow.Devices;
using Meadow.Hardware;
using System;
using System.Threading;

using ICs.IOExpanders.TCA9548A;
using Meadow.Foundation.ICs.IOExpanders;

namespace ICs.IOExpanders.TCA9685_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private Tca9548A _tca9548A;

        public MeadowApp()
        {
            Initialize();
            Run();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            var i2CBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);

            _tca9548A = new Tca9548A(i2CBus, 0x71);
        }

        public void Run()
        {
            var meadowBus = Device.CreateI2cBus(I2cBusSpeed.Standard);
            var tca = new Tca9548A(meadowBus, 0x71);
            var bus0Mcp23008 = new Mcp23x08(tca.Bus0);
            var bus1Mcp23008 = new Mcp23x08(tca.Bus1);
            var bus0Port0 = bus0Mcp23008.CreateDigitalOutputPort(bus0Mcp23008.Pins.GP0);
            var bus1Port0 = bus1Mcp23008.CreateDigitalOutputPort(bus1Mcp23008.Pins.GP0);
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
