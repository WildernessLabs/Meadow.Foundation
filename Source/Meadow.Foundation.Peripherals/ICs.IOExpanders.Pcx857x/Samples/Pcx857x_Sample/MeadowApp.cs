using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Pca9671_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Pcx857x device;

        public override Task Initialize()
        {
            device = new Pcx857x(Device.CreateI2cBus(), 0x20, Device.Pins.D01);

            return base.Initialize();
        }

        public override Task Run()
        {
            while (true)
            {
                //TestBulkDigitalOutputPortWrites(20);
                TestDigitalOutputPorts(2);
            }
        }

        private void TestDigitalOutputPorts(int loopCount)
        {
            var out00 = device.CreateDigitalOutputPort(device.Pins.R00);
            var out01 = device.CreateDigitalOutputPort(device.Pins.R01);
            var out02 = device.CreateDigitalOutputPort(device.Pins.R02);
            var out03 = device.CreateDigitalOutputPort(device.Pins.R03);

            /*
            var out04 = pca.CreateDigitalOutputPort(pca.Pins.R04);
            var out05 = pca.CreateDigitalOutputPort(pca.Pins.R05);
            var out06 = pca.CreateDigitalOutputPort(pca.Pins.R06);
            var out07 = pca.CreateDigitalOutputPort(pca.Pins.R07);
            var out08 = pca.CreateDigitalOutputPort(pca.Pins.R08);
            var out09 = pca.CreateDigitalOutputPort(pca.Pins.R09);
            var out10 = pca.CreateDigitalOutputPort(pca.Pins.R10);
            var out11 = pca.CreateDigitalOutputPort(pca.Pins.R11);
            var out12 = pca.CreateDigitalOutputPort(pca.Pins.R12);
            var out13 = pca.CreateDigitalOutputPort(pca.Pins.R13);
            var out14 = pca.CreateDigitalOutputPort(pca.Pins.R14);
            var out15 = pca.CreateDigitalOutputPort(pca.Pins.R15);

            */


            var outputPorts = new List<IDigitalOutputPort>()
            {
                out00, out01, out02, out03,
               // out04, out05, out06, out07,
               // out08, out09, out10, out11,
               // out12, out13, out14, out15
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
                    device.AllOff();

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
        //<!=SNOP=>
    }
}