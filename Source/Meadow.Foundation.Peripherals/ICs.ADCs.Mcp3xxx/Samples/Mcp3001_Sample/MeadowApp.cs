using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Mcp23s08_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Mcp3001 mcp;

        public override Task Initialize()
        {
            IDigitalOutputPort chipSelectPort = Device.CreateDigitalOutputPort(Device.Pins.D01);

            mcp = new Mcp3001(Device.CreateSpiBus(), chipSelectPort);

            return base.Initialize();
        }

        public override Task Run()
        {

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}