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
    public class MeadowApp : App<F7FeatherV1>
    {
        Mcp23x1x mcp;

        public override async Task Initialize()
        {
            Console.WriteLine("Initializing...");

            //we only want to be notified as it goes high 
            IDigitalInputPort interruptPort = Device.CreateDigitalInputPort(Device.Pins.D02, InterruptMode.EdgeBoth, ResistorMode.InternalPullDown);

            // create a new mcp with all the address pins pulled low - address 0x20 (32)
            mcp = new Mcp23x1x(Device.CreateI2cBus(), (byte)Addresses.Address_0x20, interruptPort);

        //    IDigitalOutputPort chipSelectPort = Device.CreateDigitalOutputPort(Device.Pins.D01);
        //    mcp = new Mcp23s08(Device.CreateSpiBus(), chipSelectPort, interruptPort);
        }
        
        public override Task Run()
        {
            TestInterrupts();

         //   return TestDigitalInputPorts(1000);

            return base.Run();
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
            var in00 = mcp.CreateDigitalInputPort(mcp.Pins.GPA0, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in01 = mcp.CreateDigitalInputPort(mcp.Pins.GPA1, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in02 = mcp.CreateDigitalInputPort(mcp.Pins.GPA2, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in03 = mcp.CreateDigitalInputPort(mcp.Pins.GPA3, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in04 = mcp.CreateDigitalInputPort(mcp.Pins.GPA4, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in05 = mcp.CreateDigitalInputPort(mcp.Pins.GPA5, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in06 = mcp.CreateDigitalInputPort(mcp.Pins.GPA6, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            var in07 = mcp.CreateDigitalInputPort(mcp.Pins.GPA7, InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);

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
            Console.WriteLine($"Test Interrupts {mcp != null}");

            var debounceTime = TimeSpan.FromMilliseconds(50);
            var glitchTime = TimeSpan.FromMilliseconds(0); //not used - hardware is preconfigured to 150ns

            var inputPort00 = mcp.CreateDigitalInputPort(mcp.Pins.GPA0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime, glitchTime);
            var inputPort01 = mcp.CreateDigitalInputPort(mcp.Pins.GPA1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime, glitchTime);
            var inputPort02 = mcp.CreateDigitalInputPort(mcp.Pins.GPA2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime, glitchTime);
            var inputPort03 = mcp.CreateDigitalInputPort(mcp.Pins.GPA3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime, glitchTime);
            var inputPort04 = mcp.CreateDigitalInputPort(mcp.Pins.GPA4, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime, glitchTime);
            var inputPort05 = mcp.CreateDigitalInputPort(mcp.Pins.GPA5, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime, glitchTime);
            var inputPort06 = mcp.CreateDigitalInputPort(mcp.Pins.GPA6, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime, glitchTime);
            var inputPort07 = mcp.CreateDigitalInputPort(mcp.Pins.GPA7, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime, glitchTime);

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