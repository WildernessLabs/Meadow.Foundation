using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Leds.LedBarGraph_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        LedBarGraph ledBarGraph;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // Using an array of Pins 
            IPin[] pins =
            {
                 Device.Pins.D11,
                 Device.Pins.D10,
                 Device.Pins.D09,
                 Device.Pins.D08,
                 Device.Pins.D07,
                 Device.Pins.D06,
                 Device.Pins.D05,
                 Device.Pins.D04,
                 Device.Pins.D03,
                 Device.Pins.D02
            };
            ledBarGraph = new LedBarGraph(Device, pins);

            // Passing an array of DigitalOutputPorts
            //IDigitalOutputPort[] ports =
            //{
            //Device.CreateDigitalOutputPort(Device.Pins.D05),
            //Device.CreateDigitalOutputPort(Device.Pins.D06),
            //Device.CreateDigitalOutputPort(Device.Pins.D07),
            //Device.CreateDigitalOutputPort(Device.Pins.D08),
            //Device.CreateDigitalOutputPort(Device.Pins.D09),
            //Device.CreateDigitalOutputPort(Device.Pins.D10),
            //Device.CreateDigitalOutputPort(Device.Pins.D11),
            //Device.CreateDigitalOutputPort(Device.Pins.D12),
            //Device.CreateDigitalOutputPort(Device.Pins.D13),
            //Device.CreateDigitalOutputPort(Device.Pins.D14)
            //};
            //ledBarGraph = new LedBarGraph(ports);

            TestLedBarGraph();
        }

        protected void TestLedBarGraph()
        {
            Console.WriteLine("TestLedBarGraph...");

            decimal percentage = 0;

            while (true)
            {
                Console.WriteLine("Turning them on using SetLed...");
                for (int i = 0; i < ledBarGraph.Count; i++)
                {
                    ledBarGraph.SetLed(i, true);
                    Thread.Sleep(300);
                }

                Thread.Sleep(1000);

                Console.WriteLine("Turning them off using SetLed...");
                for (int i = ledBarGraph.Count - 1; i >= 0; i--)
                {
                    ledBarGraph.SetLed(i, false);
                    Thread.Sleep(300);
                }

                Thread.Sleep(1000);

                Console.WriteLine("Turning them on using Percentage...");
                while (percentage < 1)
                {
                    percentage += 0.10m;
                    Console.WriteLine($"{percentage}");
                    ledBarGraph.Percentage = (float) Math.Min(1.0m, percentage);                    
                    Thread.Sleep(500);
                }

                Thread.Sleep(1000);

                Console.WriteLine("Turning them off using Percentage...");
                while (percentage > 0)
                {
                    percentage -= 0.10m;
                    Console.WriteLine($"{percentage}");
                    ledBarGraph.Percentage = (float) Math.Max(0.0m, percentage);                    
                    Thread.Sleep(500);
                }

                Thread.Sleep(1000);

                Console.WriteLine("Blinking for 3 seconds...");
                ledBarGraph.StartBlink();
                Thread.Sleep(3000);
                ledBarGraph.Stop();

                Thread.Sleep(1000);
            }
        }
    }
}