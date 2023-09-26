using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Pcf8574_Sample
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        //<!=SNIP=>

        private Pcf8574 device;

        public override Task Initialize()
        {
            device = new Pcf8574(Device.CreateI2cBus(1), 0x20, Device.Pins.D01);

            return base.Initialize();
        }

        public override Task Run()
        {
            TestDigitalOutputPorts(10);

            return Task.CompletedTask;
        }

        private void TestDigitalOutputPorts(int loopCount)
        {
            var out00 = device.CreateDigitalOutputPort(device.Pins.P0);
            var out01 = device.CreateDigitalOutputPort(device.Pins.P1);
            var out02 = device.CreateDigitalOutputPort(device.Pins.P2);
            var out03 = device.CreateDigitalOutputPort(device.Pins.P3);
            var out04 = device.CreateDigitalOutputPort(device.Pins.P4);
            var out05 = device.CreateDigitalOutputPort(device.Pins.P5);
            var out06 = device.CreateDigitalOutputPort(device.Pins.P6);
            var out07 = device.CreateDigitalOutputPort(device.Pins.P7);

            var outputPorts = new List<IDigitalOutputPort>()
            {
                out00, out01, out02, out03,
                out04, out05, out06, out07,
            };

            foreach (var outputPort in outputPorts)
            {
                outputPort.State = true;

                Thread.Sleep(1000);
            }

            for (int l = 0; l < loopCount; l++)
            {
                // loop through all the outputs
                for (int i = 0; i < outputPorts.Count; i++)
                {
                    // turn them all off
                    device.AllOff();

                    Thread.Sleep(1000);

                    // turn them all on
                    device.AllOn();
                    Thread.Sleep(1000);

                    // turn on just one
                    Resolver.Log.Info($"Update pin {i} to {true}");
                    outputPorts[i].State = true;
                    Thread.Sleep(250);

                    // turn off just one
                    Resolver.Log.Info($"Update pin {i} to {false}");
                    outputPorts[i].State = false;
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