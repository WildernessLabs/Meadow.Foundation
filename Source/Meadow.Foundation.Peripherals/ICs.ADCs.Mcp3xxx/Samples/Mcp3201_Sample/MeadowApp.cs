using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Mcp3001_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Mcp3201 mcp;

        IAnalogInputPort port;

        public override Task Initialize()
        {
            IDigitalOutputPort chipSelectPort = Device.CreateDigitalOutputPort(Device.Pins.D01);

            mcp = new Mcp3201(Device.CreateSpiBus(), chipSelectPort);

            port = mcp.CreateAnalogInputPort();

            return base.Initialize();
        }

        public override Task Run()
        {
            port.StartUpdating();

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}