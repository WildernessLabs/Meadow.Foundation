using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Leds.PwmLedBarGraph_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        PwmLedBarGraph pwmLedBarGraph;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // Using an array of Pins that support PWM (D02 - D13)
            //IPin[] pins =
            //{
            //     Device.Pins.D02,
            //     Device.Pins.D03,
            //     Device.Pins.D04,
            //     Device.Pins.D05,
            //     Device.Pins.D06,
            //     Device.Pins.D07,
            //     Device.Pins.D08,
            //     Device.Pins.D09,
            //     Device.Pins.D10,
            //     Device.Pins.D11
            //};
            //pwmLedBarGraph = new PwmLedBarGraph(Device, pins, 0.25f);

            // Using an array of IPwmPorts
            IPwmPort[] ports =
            {
                 Device.CreatePwmPort(Device.Pins.D02),
                 Device.CreatePwmPort(Device.Pins.D03),
                 Device.CreatePwmPort(Device.Pins.D04),
                 Device.CreatePwmPort(Device.Pins.D05),
                 Device.CreatePwmPort(Device.Pins.D06),
                 Device.CreatePwmPort(Device.Pins.D07),
                 Device.CreatePwmPort(Device.Pins.D08),
                 Device.CreatePwmPort(Device.Pins.D09),
                 Device.CreatePwmPort(Device.Pins.D10),
                 Device.CreatePwmPort(Device.Pins.D11)
            };
            pwmLedBarGraph = new PwmLedBarGraph(ports, 0.25f);            

            TestPwmLedBarGraph();
        }

        protected void TestPwmLedBarGraph()
        {
            Console.WriteLine("TestLedBarGraph...");

            float percentage = 0;

            while (true)
            {
                Console.WriteLine("Turning them on using SetLed...");
                for (int i = 0; i < pwmLedBarGraph.Count; i++)
                {
                    pwmLedBarGraph.SetLed(i, true);
                    Thread.Sleep(300);
                }

                Thread.Sleep(1000);

                Console.WriteLine("Turning them off using SetLed...");
                for (int i = pwmLedBarGraph.Count - 1; i >= 0; i--)
                {
                    pwmLedBarGraph.SetLed(i, false);
                    Thread.Sleep(300);
                }

                Thread.Sleep(1000);

                Console.WriteLine("Turning them on using Percentage...");
                while (percentage <= 1)
                {
                    percentage += 0.01f;
                    pwmLedBarGraph.Percentage = Math.Min(1.0f, percentage);
                    Thread.Sleep(100);
                }

                Thread.Sleep(1000);

                Console.WriteLine("Turning them off using Percentage...");
                while (percentage >= 0)
                {
                    percentage -= 0.01f;
                    pwmLedBarGraph.Percentage = Math.Max(0.0f, percentage);
                    Thread.Sleep(100);
                }

                Thread.Sleep(1000);
            }
        }
    }
}
