using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Tca9685_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        IDigitalOutputPort bus0Port0;
        IDigitalOutputPort bus1Port0;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.Standard);
            var tca9548a = new Tca9548a(i2cBus, 0x70);
            var mcp0 = new Mcp23008(tca9548a.Bus0);
            var mcp1 = new Mcp23008(tca9548a.Bus1);

            bus0Port0 = mcp0.CreateDigitalOutputPort(mcp0.Pins.GP0);
            bus1Port0 = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP0);

            return base.Initialize();
        }

        public override async Task Run()
        {
            while (true)
            {
                bus0Port0.State = true;
                bus1Port0.State = false;

                await Task.Delay(1000);

                bus0Port0.State = false;
                bus1Port0.State = true;

                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}
