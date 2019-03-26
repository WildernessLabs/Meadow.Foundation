using Meadow;
using Meadow.Devices;
using Meadow.Foundation.LEDs;
using Meadow.Hardware;
using System;
using System.Threading;

namespace LedBarGraph_Sample
{
    public class LedBarGraphApp : AppBase<F7Micro, LedBarGraphApp>
    {
        LedBarGraph ledBarGraph;

        public LedBarGraphApp()
        {
            IDigitalOutputPort[] ports = new IDigitalOutputPort[10];
            ports[0] = Device.CreateDigitalOutputPort(Device.Pins.D06);
            ports[1] = Device.CreateDigitalOutputPort(Device.Pins.D07);
            ports[2] = Device.CreateDigitalOutputPort(Device.Pins.D08);
            ports[3] = Device.CreateDigitalOutputPort(Device.Pins.D09);
            ports[4] = Device.CreateDigitalOutputPort(Device.Pins.D10);
            ports[5] = Device.CreateDigitalOutputPort(Device.Pins.D11);
            ports[6] = Device.CreateDigitalOutputPort(Device.Pins.D12);
            ports[7] = Device.CreateDigitalOutputPort(Device.Pins.D13);
            ports[8] = Device.CreateDigitalOutputPort(Device.Pins.D14);
            ports[9] = Device.CreateDigitalOutputPort(Device.Pins.D15);
            ledBarGraph = new LedBarGraph(ports);

            TestLedBarGraph();
        }

        protected void TestLedBarGraph()
        {
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

                float percentage = 0;
                Console.WriteLine("Turning them on using Percentage...");
                while (percentage < 1)
                {
                    ledBarGraph.Percentage = percentage;
                    percentage += 0.10f;
                    Thread.Sleep(200);
                }

                Thread.Sleep(1000);

                percentage = 1.0f;
                Console.WriteLine("Turning them off using Percentage...");
                while (percentage > 0)
                {
                    ledBarGraph.Percentage = percentage;
                    percentage -= 0.10f;
                    Thread.Sleep(200);
                }

                Thread.Sleep(1000);
            }
        }
    }
}