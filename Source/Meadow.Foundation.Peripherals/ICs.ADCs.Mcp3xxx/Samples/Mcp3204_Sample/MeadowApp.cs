using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Mcp3204_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Mcp3204 mcp;

        IAnalogInputPort port;

        public override Task Initialize()
        {
            IDigitalOutputPort chipSelectPort = Device.CreateDigitalOutputPort(Device.Pins.D01);

            mcp = new Mcp3204(Device.CreateSpiBus(), chipSelectPort);

            port = mcp.CreateAnalogInputPort(mcp.Pins.CH0, 32, TimeSpan.FromSeconds(1), new Voltage(3.3, Voltage.UnitType.Volts), Mcp3xxx.InputType.SingleEnded);

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