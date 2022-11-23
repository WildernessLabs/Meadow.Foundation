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
        Mcp23008 mcp;

        public override Task Initialize()
        {
            Console.WriteLine("Initializing...");

            
            IDigitalInputPort interruptPort = Device.CreateDigitalInputPort(Device.Pins.D00, InterruptMode.EdgeBoth, ResistorMode.InternalPullDown);
            IDigitalOutputPort resetPort = Device.CreateDigitalOutputPort(Device.Pins.D01);

            mcp = new Mcp23008(Device.CreateI2cBus(), (byte)Addresses.Address_0x20, interruptPort, resetPort);

            return base.Initialize();
        }
        
        public override Task Run()
        {
           // TestInterrupts();

            return TestDigitalInputPorts(1000);

            //return base.Run();
        }

        void TestBulkPinReads(int loopCount)
        {
            for (int l = 0; l < loopCount; l++) 
            {
                byte mask = mcp.ReadFromPorts();
                var bits = new BitArray(new byte[] { mask });

                var bitsString = new StringBuilder();
            
                foreach (var bit in bits) 
                {
                    bitsString.Append((bool)bit ? "1":"0");
                }

                Console.WriteLine($"Port Values, raw:{mask:X}, bits: { bitsString}");

                Thread.Sleep(100);
            }
        }

        async Task TestDigitalInputPorts(int loopCount)
        {
            var in00 = mcp.CreateDigitalInputPort(mcp.Pins.GP0, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in01 = mcp.CreateDigitalInputPort(mcp.Pins.GP1, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in02 = mcp.CreateDigitalInputPort(mcp.Pins.GP2, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in03 = mcp.CreateDigitalInputPort(mcp.Pins.GP3, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in04 = mcp.CreateDigitalInputPort(mcp.Pins.GP4, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in05 = mcp.CreateDigitalInputPort(mcp.Pins.GP5, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in06 = mcp.CreateDigitalInputPort(mcp.Pins.GP6, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in07 = mcp.CreateDigitalInputPort(mcp.Pins.GP7, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);

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
                    //Console.WriteLine($"InputPort {inputPort.Pin.Name} Read: {inputPort.State}");
                    output += $"{(inputPort.State ? 1 : 0)}";
                }
                Console.WriteLine(output);
                await Task.Delay(500);
            }

            // cleanup
            for (int i = 0; i < inputPorts.Count; i++) 
            {
                inputPorts[i].Dispose();
            }
        }

        void TestInterrupts()
        {
            Console.WriteLine($"Test Interrupts");

            var debounceTime = TimeSpan.FromMilliseconds(50);

            var inputPort00 = mcp.CreateDigitalInputPort(mcp.Pins.GP0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort01 = mcp.CreateDigitalInputPort(mcp.Pins.GP1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort02 = mcp.CreateDigitalInputPort(mcp.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort03 = mcp.CreateDigitalInputPort(mcp.Pins.GP3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort04 = mcp.CreateDigitalInputPort(mcp.Pins.GP4, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort05 = mcp.CreateDigitalInputPort(mcp.Pins.GP5, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort06 = mcp.CreateDigitalInputPort(mcp.Pins.GP6, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort07 = mcp.CreateDigitalInputPort(mcp.Pins.GP7, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);

            inputPort00.Changed += (s, e) => Console.WriteLine($"Port 0 interrupt {e.New.State}");
            inputPort01.Changed += (s, e) => Console.WriteLine($"Port 1 interrupt {e.New.State}");
            inputPort02.Changed += (s, e) => Console.WriteLine($"Port 2 interrupt {e.New.State}");
            inputPort03.Changed += (s, e) => Console.WriteLine($"Port 3 interrupt {e.New.State}");
            inputPort04.Changed += (s, e) => Console.WriteLine($"Port 4 interrupt {e.New.State}");
            inputPort05.Changed += (s, e) => Console.WriteLine($"Port 5 interrupt {e.New.State}");
            inputPort06.Changed += (s, e) => Console.WriteLine($"Port 6 interrupt {e.New.State}");
            inputPort07.Changed += (s, e) => Console.WriteLine($"Port 7 interrupt {e.New.State}");
        }
    }
}