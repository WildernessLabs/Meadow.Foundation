using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using System;
using System.Threading;

namespace LedBarGraph_Sample
{
    public class LedBarGraphApp : App<F7Micro, LedBarGraphApp>
    {
        LedBarGraph ledBarGraph;

        public LedBarGraphApp()
        {
            IDigitalOutputPort[] ports = 
            {
                 Device.CreateDigitalOutputPort(Device.Pins.D06),
                 Device.CreateDigitalOutputPort(Device.Pins.D07),
                 Device.CreateDigitalOutputPort(Device.Pins.D08),
                 Device.CreateDigitalOutputPort(Device.Pins.D09),
                 Device.CreateDigitalOutputPort(Device.Pins.D10),
                 Device.CreateDigitalOutputPort(Device.Pins.D11),
                 Device.CreateDigitalOutputPort(Device.Pins.D01),
                 Device.CreateDigitalOutputPort(Device.Pins.D00),
                 Device.CreateDigitalOutputPort(Device.Pins.D14),
                 Device.CreateDigitalOutputPort(Device.Pins.D15)
            }; 

            ledBarGraph = new LedBarGraph(ports);

            TestLedBarGraph();
        }

        protected void TestLedBarGraph()
        {
            float percentage = 0;

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

                Console.WriteLine("Turning them on using Percentage...");
                while (percentage <= 1)
                {
                    percentage += 0.10f;
                    ledBarGraph.Percentage = Math.Min(1.0f, percentage);                    
                    Thread.Sleep(100);
                }

                Thread.Sleep(1000);

                Console.WriteLine("Turning them off using Percentage...");
                while (percentage >= 0)
                {
                    percentage -= 0.10f;
                    ledBarGraph.Percentage = Math.Max(0.0f, percentage); ;                    
                    Thread.Sleep(100);
                }

                Thread.Sleep(1000);
            }
        }
    }
}