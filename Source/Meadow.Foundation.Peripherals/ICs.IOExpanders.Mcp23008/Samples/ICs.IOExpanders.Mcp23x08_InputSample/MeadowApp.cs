using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;

namespace ICs.IOExpanders.Mcp23x08_InputSample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Mcp23x08 _mcp;

        public MeadowApp()
        {
            Console.WriteLine("Initializing.");

            ConfigurePeripherals();

            TestInterrupts();

            //while (true) {
            //    TestBulkPinReads(10);
            //    TestDigitalInputPorts(10);
            //}

        }

        public void ConfigurePeripherals()
        {
            IDigitalInputPort interruptPort =
                Device.CreateDigitalInputPort(
                    Device.Pins.D00,
                    InterruptMode.EdgeRising);
            // create a new mcp with all the address pins pulled low for
            // an address of 0x20/32
            _mcp = new Mcp23x08(Device.CreateI2cBus(), false, false, false, interruptPort);
        }

        void TestBulkPinReads(int loopCount)
        {
            for (int l = 0; l < loopCount; l++) {
                byte mask = _mcp.ReadFromPorts();
                var bits = new BitArray(new byte[] { mask });
                StringBuilder bitsString = new StringBuilder();
                foreach (var bit in bits) {
                    bitsString.Append((bool)bit?"1":"0");
                }

                Console.WriteLine($"Port Values, raw:{mask.ToString("X")}, bits: { bitsString.ToString()}");

                Thread.Sleep(100);
            }
        }

        void TestDigitalInputPorts(int loopCount)
        {
            var in00 = _mcp.CreateDigitalInputPort(_mcp.Pins.GP0);
            var in01 = _mcp.CreateDigitalInputPort(_mcp.Pins.GP1);
            var in02 = _mcp.CreateDigitalInputPort(_mcp.Pins.GP2);
            var in03 = _mcp.CreateDigitalInputPort(_mcp.Pins.GP3);
            var in04 = _mcp.CreateDigitalInputPort(_mcp.Pins.GP4);
            var in05 = _mcp.CreateDigitalInputPort(_mcp.Pins.GP5);
            var in06 = _mcp.CreateDigitalInputPort(_mcp.Pins.GP6);
            var in07 = _mcp.CreateDigitalInputPort(_mcp.Pins.GP7);

            var innies = new List<IDigitalInputPort>() {
                in00, in01, in02, in03, in04, in05, in06, in07
            };

            // read all the ports, sleep for 100ms and repeat a few times.
            for (int l = 0; l < loopCount; l++) {
                foreach (var innie in innies) {
                    Console.WriteLine($"InputPort {innie.Pin.Name} Read: {innie.State}");
                }
                Thread.Sleep(100);
            }

            // cleanup
            for (int i = 0; i < innies.Count; i++) {
                innies[i].Dispose();
            }
        }

        void TestInterrupts()
        {
            Console.WriteLine("Test Interrupts");
            var in00 = _mcp.CreateDigitalInputPort(_mcp.Pins.GP0, InterruptMode.EdgeRising);
            //var in01 = _mcp.CreateDigitalInputPort(_mcp.Pins.GP1);
            //var in02 = _mcp.CreateDigitalInputPort(_mcp.Pins.GP2);
            //var in03 = _mcp.CreateDigitalInputPort(_mcp.Pins.GP3);
            //var in04 = _mcp.CreateDigitalInputPort(_mcp.Pins.GP4);
            //var in05 = _mcp.CreateDigitalInputPort(_mcp.Pins.GP5);
            //var in06 = _mcp.CreateDigitalInputPort(_mcp.Pins.GP6);
            //var in07 = _mcp.CreateDigitalInputPort(_mcp.Pins.GP7);

            in00.Changed += (s,e) => {
                Console.WriteLine("changed event");
            };
        }
    }
}
