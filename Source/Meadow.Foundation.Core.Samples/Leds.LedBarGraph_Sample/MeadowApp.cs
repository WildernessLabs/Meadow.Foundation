using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace Leds.LedBarGraph_Sample
{
    public class MeadowApp : App<F7FeatherV1>
    {
        //<!=SNIP=>

        LedBarGraph ledBarGraph;

        public override Task Initialize()
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

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Console.WriteLine("TestLedBarGraph...");

            float percentage = 0;

            while (true)
            {
                Console.WriteLine("Turning them on and off for 200ms using SetLed...");
                for (int i = 0; i < ledBarGraph.Count; i++)
                {
                    ledBarGraph.SetLed(i, true);
                    await Task.Delay(100);
                    ledBarGraph.SetLed(i, false);
                }

                await Task.Delay(1000);

                Console.WriteLine("Turning them on using Percentage...");
                while (percentage < 1)
                {
                    percentage += 0.10f;
                    Console.WriteLine($"{percentage}");
                    ledBarGraph.Percentage = Math.Min(1.0f, percentage);
                    await Task.Delay(100);
                }

                await Task.Delay(1000);

                Console.WriteLine("Turning them off using Percentage...");
                while (percentage > 0)
                {
                    percentage -= 0.10f;
                    Console.WriteLine($"{percentage}");
                    ledBarGraph.Percentage = Math.Max(0.0f, percentage);
                    await Task.Delay(100);
                }

                await Task.Delay(1000);

                Console.WriteLine("Charging animation...");
                while (percentage < 1)
                {
                    percentage += 0.10f;
                    Console.WriteLine($"{percentage}");
                    ledBarGraph.Percentage = Math.Min(1.0f, percentage);
                    ledBarGraph.StartBlink(ledBarGraph.GetTopLedForPercentage());
                    await Task.Delay(2000);
                }

                await Task.Delay(1000);

                Console.WriteLine("Discharging animation...");
                while (percentage > 0)
                {
                    percentage -= 0.10f;
                    Console.WriteLine($"{percentage}");
                    ledBarGraph.Percentage = Math.Max(0.0f, percentage);
                    ledBarGraph.StartBlink(ledBarGraph.GetTopLedForPercentage());
                    await Task.Delay(2000);
                }

                await Task.Delay(1000);

                Console.WriteLine("Blinking for 5 seconds at 500ms on/off...");
                ledBarGraph.StartBlink();
                await Task.Delay(5000);
                ledBarGraph.Stop();

                await Task.Delay(1000);

                Console.WriteLine("Blinking for 5 seconds at 200ms on/off...");
                ledBarGraph.StartBlink(TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(200));
                await Task.Delay(5000);
                ledBarGraph.Stop();

                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}