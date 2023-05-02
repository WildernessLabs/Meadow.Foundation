using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Mcp23008_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Mcp23008 mcp;

        public override Task Initialize()
        {
            IDigitalInputPort interruptPort = Device.CreateDigitalInputPort(Device.Pins.D00, InterruptMode.EdgeRising);
            IDigitalOutputPort resetPort = Device.CreateDigitalOutputPort(Device.Pins.D01);

            mcp = new Mcp23008(Device.CreateI2cBus(), 0x20, interruptPort, resetPort);

            return base.Initialize();
        }

        public override Task Run()
        {
            BenchmarkDirectDigitalOutputPorts();
            BenchmarkDigitalOutputPorts();
            while (true)
            {
                TestBulkDigitalOutputPortWrites(20);
                TestDigitalOutputPorts(2);
            }
        }

        private void BenchmarkDirectDigitalOutputPorts()
        {
            var out00 = Device.CreateDigitalOutputPort(Device.Pins.D14);
            var out01 = Device.CreateDigitalOutputPort(Device.Pins.D13);
            var out02 = Device.CreateDigitalOutputPort(Device.Pins.D12);
            var out03 = Device.CreateDigitalOutputPort(Device.Pins.D11);
            var out04 = Device.CreateDigitalOutputPort(Device.Pins.D10);
            var out05 = Device.CreateDigitalOutputPort(Device.Pins.D09);
            var out06 = Device.CreateDigitalOutputPort(Device.Pins.D03);
            var out07 = Device.CreateDigitalOutputPort(Device.Pins.D02);
            
            var outputPorts = new List<IDigitalOutputPort>() 
            {
                out00, out01, out02, out03, out04, out05, out06, out07
            };

            var state = false;
            var stopwatch = new Stopwatch();
            Console.WriteLine("Starting benchmark");

            for (var x = 0; x < 10; x++)
            {
                stopwatch.Restart();
                for (var iteration = 0; iteration < 100; iteration++)
                {
                    for (var i = 0; i < outputPorts.Count; i++)
                    {
                        outputPorts[i].State = state;
                    }

                    state = !state;
                }
                stopwatch.Stop();
                
                Console.WriteLine($"{100 * outputPorts.Count} pins toggled in {stopwatch.ElapsedMilliseconds}ms");
            }
            
            Console.WriteLine("Benchmark finished");
        }

        private void BenchmarkDigitalOutputPorts()
        {
            var out00 = mcp.CreateDigitalOutputPort(mcp.Pins.GP0);
            var out01 = mcp.CreateDigitalOutputPort(mcp.Pins.GP1);
            var out02 = mcp.CreateDigitalOutputPort(mcp.Pins.GP2);
            var out03 = mcp.CreateDigitalOutputPort(mcp.Pins.GP3);
            var out04 = mcp.CreateDigitalOutputPort(mcp.Pins.GP4);
            var out05 = mcp.CreateDigitalOutputPort(mcp.Pins.GP5);
            var out06 = mcp.CreateDigitalOutputPort(mcp.Pins.GP6);
            var out07 = mcp.CreateDigitalOutputPort(mcp.Pins.GP7);

            var outputPorts = new List<IDigitalOutputPort>() 
            {
                out00, out01, out02, out03, out04, out05, out06, out07
            };

            var state = false;
            var stopwatch = new Stopwatch();
            Console.WriteLine("Starting benchmark");

            for (var x = 0; x < 10; x++)
            {
                stopwatch.Restart();
                for (var iteration = 0; iteration < 100; iteration++)
                {
                    for (var i = 0; i < outputPorts.Count; i++)
                    {
                        outputPorts[i].State = state;
                    }

                    state = !state;
                }
                stopwatch.Stop();
                
                Console.WriteLine($"{100 * outputPorts.Count} pins toggled in {stopwatch.ElapsedMilliseconds}ms");
            }
            
            Console.WriteLine("Benchmark finished");
        }

        void TestDigitalOutputPorts(int loopCount)
        {
            var out00 = mcp.CreateDigitalOutputPort(mcp.Pins.GP0);
            var out01 = mcp.CreateDigitalOutputPort(mcp.Pins.GP1);
            var out02 = mcp.CreateDigitalOutputPort(mcp.Pins.GP2);
            var out03 = mcp.CreateDigitalOutputPort(mcp.Pins.GP3);
            var out04 = mcp.CreateDigitalOutputPort(mcp.Pins.GP4);
            var out05 = mcp.CreateDigitalOutputPort(mcp.Pins.GP5);
            var out06 = mcp.CreateDigitalOutputPort(mcp.Pins.GP6);
            var out07 = mcp.CreateDigitalOutputPort(mcp.Pins.GP7);

            var outputPorts = new List<IDigitalOutputPort>() 
            {
                out00, out01, out02, out03, out04, out05, out06, out07
            };

            foreach (var outputPort in outputPorts)
            {
                outputPort.State = true;
            }

            for(int l = 0; l < loopCount; l++) 
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

        void TestBulkDigitalOutputPortWrites(int loopCount)
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