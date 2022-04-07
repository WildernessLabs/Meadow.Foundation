using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System.Collections.Generic;
using System.Threading;

namespace ICs.IOExpanders.Mcp23x08_Sample
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        //<!=SNIP=>

        Mcp23x08 _mcp;
        public MeadowApp()
        {
            TestOutputs();            
        }

        void TestOutputs() 
        {
            InitializeOutputs();

            while (true)
            {
                TestBulkDigitalOutputPortWrites(20);
                TestDigitalOutputPorts(2);
            }
        }

        void InitializeOutputs()
        {
            IDigitalInputPort interruptPort =
                Device.CreateDigitalInputPort(
                    Device.Pins.D00,
                    InterruptMode.EdgeRising);
            // create a new mcp with all the address pins pulled low for
            // an address of 0x20/32
            _mcp = new Mcp23x08(Device.CreateI2cBus(), false, false, false, interruptPort);
        }

        void TestDigitalOutputPorts(int loopCount)
        {
            var out00 = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP0);
            var out01 = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP1);
            var out02 = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP2);
            var out03 = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP3);
            var out04 = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP4);
            var out05 = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP5);
            var out06 = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP6);
            var out07 = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP7);

            var outs = new List<IDigitalOutputPort>() {
                out00, out01, out02, out03, out04, out05, out06, out07
            };

            foreach (var outie in outs) {
                outie.State = true;
            }

            for(int l = 0; l < loopCount; l++) {
                // loop through all the outputs
                for (int i = 0; i < outs.Count; i++) {
                    // turn them all off
                    foreach (var outie in outs) { outie.State = false; }

                    // turn on just one
                    outs[i].State = true;
                    Thread.Sleep(250);
                }
            }

            // cleanup
            for (int i = 0; i < outs.Count; i++) {
                outs[i].Dispose();
            }
        }

        void TestBulkDigitalOutputPortWrites(int loopCount)
        {
            byte mask = 0x0;

            for (int l = 0; l < loopCount; l++) {

                for (int i = 0; i < 8; i++) {
                    _mcp.WriteToPorts(mask);
                    mask = (byte)(1 << i);
                    Thread.Sleep(5);
                }
            }
        }

        //<!=SNOP=>
    }
}
