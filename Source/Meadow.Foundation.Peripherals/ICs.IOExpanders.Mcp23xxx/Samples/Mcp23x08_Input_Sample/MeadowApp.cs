using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Meadow.Foundation.ICs.IOExpanders.Mcp23xxx;

namespace ICs.IOExpanders.Mcp23x08_Input_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        private Mcp23008 mcp;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");


            var interruptPort = Device.CreateDigitalInterruptPort(Device.Pins.D00, InterruptMode.EdgeBoth, ResistorMode.InternalPullDown);
            var resetPort = Device.CreateDigitalOutputPort(Device.Pins.D01);

            mcp = new Mcp23008(Device.CreateI2cBus(), (byte)Addresses.Address_0x20, interruptPort, resetPort);

            return base.Initialize();
        }

        public override Task Run()
        {
            // TestInterrupts();

            return TestDigitalInputPorts(1000);

            //return base.Run();
        }

        private void TestBulkPinReads(int loopCount)
        {
            for (int l = 0; l < loopCount; l++)
            {
                byte mask = mcp.ReadFromPorts();
                var bits = new BitArray(new byte[] { mask });

                var bitsString = new StringBuilder();

                foreach (var bit in bits)
                {
                    bitsString.Append((bool)bit ? "1" : "0");
                }

                Resolver.Log.Info($"Port Values, raw:{mask:X}, bits: {bitsString}");

                Thread.Sleep(100);
            }
        }

        private async Task TestDigitalInputPorts(int loopCount)
        {
            var in00 = mcp.CreateDigitalInterruptPort(mcp.Pins.GP0, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in01 = mcp.CreateDigitalInterruptPort(mcp.Pins.GP1, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in02 = mcp.CreateDigitalInterruptPort(mcp.Pins.GP2, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in03 = mcp.CreateDigitalInterruptPort(mcp.Pins.GP3, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in04 = mcp.CreateDigitalInterruptPort(mcp.Pins.GP4, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in05 = mcp.CreateDigitalInterruptPort(mcp.Pins.GP5, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in06 = mcp.CreateDigitalInterruptPort(mcp.Pins.GP6, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in07 = mcp.CreateDigitalInterruptPort(mcp.Pins.GP7, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);

            var inputPorts = new List<IDigitalInputPort>()
            {
                in00, in01, in02, in03, in04, in05, in06, in07
            };

            string output;

            // read all the ports, sleep for 100ms and repeat a few times.
            for (int l = 0; l < loopCount; l++)
            {
                output = string.Empty;

                foreach (var inputPort in inputPorts)
                {
                    //Resolver.Log.Info($"InputPort {inputPort.Pin.Name} Read: {inputPort.State}");
                    output += $"{(inputPort.State ? 1 : 0)}";
                }
                Resolver.Log.Info(output);
                await Task.Delay(500);
            }

            // cleanup
            for (int i = 0; i < inputPorts.Count; i++)
            {
                inputPorts[i].Dispose();
            }
        }

        private void TestInterrupts()
        {
            Resolver.Log.Info($"Test Interrupts");

            var debounceTime = TimeSpan.FromMilliseconds(50);

            var inputPort00 = mcp.CreateDigitalInterruptPort(mcp.Pins.GP0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort01 = mcp.CreateDigitalInterruptPort(mcp.Pins.GP1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort02 = mcp.CreateDigitalInterruptPort(mcp.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort03 = mcp.CreateDigitalInterruptPort(mcp.Pins.GP3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort04 = mcp.CreateDigitalInterruptPort(mcp.Pins.GP4, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort05 = mcp.CreateDigitalInterruptPort(mcp.Pins.GP5, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort06 = mcp.CreateDigitalInterruptPort(mcp.Pins.GP6, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort07 = mcp.CreateDigitalInterruptPort(mcp.Pins.GP7, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);

            inputPort00.Changed += (s, e) => Resolver.Log.Info($"Port 0 interrupt {e.New.State}");
            inputPort01.Changed += (s, e) => Resolver.Log.Info($"Port 1 interrupt {e.New.State}");
            inputPort02.Changed += (s, e) => Resolver.Log.Info($"Port 2 interrupt {e.New.State}");
            inputPort03.Changed += (s, e) => Resolver.Log.Info($"Port 3 interrupt {e.New.State}");
            inputPort04.Changed += (s, e) => Resolver.Log.Info($"Port 4 interrupt {e.New.State}");
            inputPort05.Changed += (s, e) => Resolver.Log.Info($"Port 5 interrupt {e.New.State}");
            inputPort06.Changed += (s, e) => Resolver.Log.Info($"Port 6 interrupt {e.New.State}");
            inputPort07.Changed += (s, e) => Resolver.Log.Info($"Port 7 interrupt {e.New.State}");
        }
    }
}