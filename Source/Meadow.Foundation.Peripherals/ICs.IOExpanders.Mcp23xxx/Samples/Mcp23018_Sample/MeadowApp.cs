using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Mcp23018_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Mcp23018 mcp;

        public override Task Initialize()
        {
            var interruptPort = Device.CreateDigitalInterruptPort(Device.Pins.D00, InterruptMode.EdgeRising);
            var resetPort = Device.CreateDigitalOutputPort(Device.Pins.D01);

            mcp = new Mcp23018(Device.CreateI2cBus(), 0x20, interruptPort, resetPort);

            return base.Initialize();
        }

        public override Task Run()
        {
            while (true)
            {
                TestBulkDigitalOutputPortWrites(20);
                TestDigitalOutputPorts(2);
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

            var outputPorts = new List<IDigitalOutputPort>()
            {
                out00, out01, out02, out03, out04, out05, out06, out07
            };

            foreach (var outputPort in outputPorts)
            {
                outputPort.State = true;
            }

            for (int l = 0; l < loopCount; l++)
            {
                // loop through all the outputs
                for (int i = 0; i < outputPorts.Count; i++)
                {
                    // turn them all off
                    foreach (var outputPort in outputPorts)
                    {
                        outputPort.State = false;
                    }

                    // turn on just one
                    outputPorts[i].State = true;
                    Thread.Sleep(250);
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