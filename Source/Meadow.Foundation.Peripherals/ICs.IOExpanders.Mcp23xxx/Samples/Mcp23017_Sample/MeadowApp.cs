using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Mcp23017_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Mcp23017 mcp;

        public override Task Initialize()
        {
            var interruptPort = Device.CreateDigitalInterruptPort(Device.Pins.D00, InterruptMode.EdgeRising);
            var resetPort = Device.CreateDigitalOutputPort(Device.Pins.D01);

            mcp = new Mcp23017(Device.CreateI2cBus(), 0x20, interruptPort, resetPort);

            return base.Initialize();
        }

        public override Task Run()
        {
            while (true)
            {
                //TestBulkDigitalOutputPortWrites(20);
                TestDigitalOutputPorts(20);
            }
        }

        private void TestDigitalOutputPorts(int loopCount)
        {
            var out00 = mcp.CreateDigitalOutputPort(mcp.Pins.GPA0);
            var out01 = mcp.CreateDigitalOutputPort(mcp.Pins.GPA1);
            var out02 = mcp.CreateDigitalOutputPort(mcp.Pins.GPA2);
            var out03 = mcp.CreateDigitalOutputPort(mcp.Pins.GPA3);
            var out04 = mcp.CreateDigitalOutputPort(mcp.Pins.GPA4);
            var out05 = mcp.CreateDigitalOutputPort(mcp.Pins.GPA5);
            var out06 = mcp.CreateDigitalOutputPort(mcp.Pins.GPA6);
            var out07 = mcp.CreateDigitalOutputPort(mcp.Pins.GPA7);

            var out10 = mcp.CreateDigitalOutputPort(mcp.Pins.GPB0);
            var out11 = mcp.CreateDigitalOutputPort(mcp.Pins.GPB1);
            var out12 = mcp.CreateDigitalOutputPort(mcp.Pins.GPB2);
            var out13 = mcp.CreateDigitalOutputPort(mcp.Pins.GPB3);
            var out14 = mcp.CreateDigitalOutputPort(mcp.Pins.GPB4);
            var out15 = mcp.CreateDigitalOutputPort(mcp.Pins.GPB5);
            var out16 = mcp.CreateDigitalOutputPort(mcp.Pins.GPB6);
            var out17 = mcp.CreateDigitalOutputPort(mcp.Pins.GPB7);

            var outputPorts = new List<IDigitalOutputPort>()
            {
                out00, out01, out02, out03, out04, out05, out06, out07,
                out10, out11, out12, out13, out14, out15, out16, out17
            };

            for (int l = 0; l < loopCount; l++)
            {
                foreach (var outputPort in outputPorts)
                {
                    Resolver.Log.Info($"{outputPort.Pin.Name} on");
                    outputPort.State = true;
                    Thread.Sleep(500);
                    Resolver.Log.Info($"{outputPort.Pin.Name} off");
                    outputPort.State = false;
                    Thread.Sleep(500);
                }
            }

            // cleanup
            for (int i = 0; i < outputPorts.Count; i++)
            {
                outputPorts[i].Dispose();
            }
        }

        private void TestBulkDigitalOutputPortWrites(int loopCount)
        {
            byte mask = 0x0;

            for (int l = 0; l < loopCount; l++)
            {
                for (int i = 0; i < 8; i++)
                {
                    mcp.WriteToPorts(mask);
                    mask = (byte)(1 << i);
                    Thread.Sleep(5);
                }
            }
        }
        //<!=SNOP=>
    }
}