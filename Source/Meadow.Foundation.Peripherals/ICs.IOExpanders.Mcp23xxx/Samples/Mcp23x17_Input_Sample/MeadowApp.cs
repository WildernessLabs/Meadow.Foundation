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

namespace ICs.IOExpanders.Mcp23x17_Input_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        Mcp23017 mcp;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            IDigitalInputPort interruptPort = Device.CreateDigitalInputPort(Device.Pins.D00, InterruptMode.EdgeBoth, ResistorMode.InternalPullDown);
            IDigitalOutputPort resetPort = Device.CreateDigitalOutputPort(Device.Pins.D01);

            mcp = new Mcp23017(Device.CreateI2cBus(), (byte)Addresses.Address_0x20, interruptPort, resetPort);

            mcp.InputChanged += Mcp_InputChanged;

            return base.Initialize();
        }

        private void Mcp_InputChanged(object sender, IOExpanderInputChangedEventArgs e)
        {
            Resolver.Log.Info($"Changed: {e.InterruptPins}");
        }

        public override Task Run()
        {
            TestInterrupts();

        //    return TestDigitalInputPorts(1000);

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

                Resolver.Log.Info($"Port Values, raw:{mask:X}, bits: { bitsString}");

                Thread.Sleep(100);
            }
        }

        async Task TestDigitalInputPorts(int loopCount)
        {
            var in00 = mcp.CreateDigitalInputPort(mcp.Pins.GPA0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            var in01 = mcp.CreateDigitalInputPort(mcp.Pins.GPA1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            var in02 = mcp.CreateDigitalInputPort(mcp.Pins.GPA2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            var in03 = mcp.CreateDigitalInputPort(mcp.Pins.GPA3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            var in04 = mcp.CreateDigitalInputPort(mcp.Pins.GPA4, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            var in05 = mcp.CreateDigitalInputPort(mcp.Pins.GPA5, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            var in06 = mcp.CreateDigitalInputPort(mcp.Pins.GPA6, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            var in07 = mcp.CreateDigitalInputPort(mcp.Pins.GPA7, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);

            var in08 = mcp.CreateDigitalInputPort(mcp.Pins.GPB0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            var in09 = mcp.CreateDigitalInputPort(mcp.Pins.GPB1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            var in10 = mcp.CreateDigitalInputPort(mcp.Pins.GPB2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            var in11 = mcp.CreateDigitalInputPort(mcp.Pins.GPB3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            var in12 = mcp.CreateDigitalInputPort(mcp.Pins.GPB4, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            var in13 = mcp.CreateDigitalInputPort(mcp.Pins.GPB5, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            var in14 = mcp.CreateDigitalInputPort(mcp.Pins.GPB6, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            var in15 = mcp.CreateDigitalInputPort(mcp.Pins.GPB7, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);

            var inputPorts = new List<IDigitalInputPort>() 
            {
                in00, in01, in02, in03, in04, in05, in06, in07, 
                in08, in09, in10, in11, in12, in13, in14, in15,
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

        void TestInterrupts()
        {
            Resolver.Log.Info($"Test Interrupts");

            var debounceTime = TimeSpan.FromMilliseconds(50);

            var inputPort00 = mcp.CreateDigitalInputPort(mcp.Pins.GPA0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort01 = mcp.CreateDigitalInputPort(mcp.Pins.GPA1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort02 = mcp.CreateDigitalInputPort(mcp.Pins.GPA2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort03 = mcp.CreateDigitalInputPort(mcp.Pins.GPA3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort04 = mcp.CreateDigitalInputPort(mcp.Pins.GPA4, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort05 = mcp.CreateDigitalInputPort(mcp.Pins.GPA5, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort06 = mcp.CreateDigitalInputPort(mcp.Pins.GPA6, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort07 = mcp.CreateDigitalInputPort(mcp.Pins.GPA7, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
        
            var inputPort08 = mcp.CreateDigitalInputPort(mcp.Pins.GPB0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort09 = mcp.CreateDigitalInputPort(mcp.Pins.GPB1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort10 = mcp.CreateDigitalInputPort(mcp.Pins.GPB2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort11 = mcp.CreateDigitalInputPort(mcp.Pins.GPB3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort12 = mcp.CreateDigitalInputPort(mcp.Pins.GPB4, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort13 = mcp.CreateDigitalInputPort(mcp.Pins.GPB5, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort14 = mcp.CreateDigitalInputPort(mcp.Pins.GPB6, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);
            var inputPort15 = mcp.CreateDigitalInputPort(mcp.Pins.GPB7, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp, debounceTime);

            inputPort00.Changed += (s, e) => Resolver.Log.Info($"Port A0 interrupt {e.New.State}");
            inputPort01.Changed += (s, e) => Resolver.Log.Info($"Port A1 interrupt {e.New.State}");
            inputPort02.Changed += (s, e) => Resolver.Log.Info($"Port A2 interrupt {e.New.State}");
            inputPort03.Changed += (s, e) => Resolver.Log.Info($"Port A3 interrupt {e.New.State}");
            inputPort04.Changed += (s, e) => Resolver.Log.Info($"Port A4 interrupt {e.New.State}");
            inputPort05.Changed += (s, e) => Resolver.Log.Info($"Port A5 interrupt {e.New.State}");
            inputPort06.Changed += (s, e) => Resolver.Log.Info($"Port A6 interrupt {e.New.State}");
            inputPort07.Changed += (s, e) => Resolver.Log.Info($"Port A7 interrupt {e.New.State}");

            inputPort08.Changed += (s, e) => Resolver.Log.Info($"Port B0 interrupt {e.New.State}");
            inputPort09.Changed += (s, e) => Resolver.Log.Info($"Port B1 interrupt {e.New.State}");
            inputPort10.Changed += (s, e) => Resolver.Log.Info($"Port B2 interrupt {e.New.State}");
            inputPort11.Changed += (s, e) => Resolver.Log.Info($"Port B3 interrupt {e.New.State}");
            inputPort12.Changed += (s, e) => Resolver.Log.Info($"Port B4 interrupt {e.New.State}");
            inputPort13.Changed += (s, e) => Resolver.Log.Info($"Port B5 interrupt {e.New.State}");
            inputPort14.Changed += (s, e) => Resolver.Log.Info($"Port B6 interrupt {e.New.State}");
            inputPort15.Changed += (s, e) => Resolver.Log.Info($"Port B7 interrupt {e.New.State}");
        }
    }
}