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

            decimal percentage = 0;

            while (true)
            {
                Console.WriteLine("Turning them on and off for 1 second using SetLed...");
                for (int i = 0; i < ledBarGraph.Count; i++)
                {
                    ledBarGraph.SetLed(i, true);
                    await Task.Delay(1000);
                    ledBarGraph.SetLed(i, false);
                }

                await Task.Delay(1000);

                Console.WriteLine("Turning them on using Percentage...");
                while (percentage < 1)
                {
                    percentage += 0.10m;
                    Console.WriteLine($"{percentage}");
                    ledBarGraph.Percentage = (float) Math.Min(1.0m, percentage);
                    await Task.Delay(1000);
                }

                await Task.Delay(1000);

                Console.WriteLine("Turning them off using Percentage...");
                while (percentage > 0)
                {
                    percentage -= 0.10m;
                    Console.WriteLine($"{percentage}");
                    ledBarGraph.Percentage = (float) Math.Max(0.0m, percentage);
                    await Task.Delay(1000);
                }

                await Task.Delay(1000);

                Console.WriteLine("Charging animation...");
                while (percentage < 1)
                {
                    percentage += 0.10m;
                    Console.WriteLine($"{percentage}");
                    ledBarGraph.Percentage = (float)Math.Min(1.0m, percentage);
                    ledBarGraph.SetLedBlink(ledBarGraph.GetTopLedForPercentage());
                    await Task.Delay(1000);
                }

                await Task.Delay(1000);

                Console.WriteLine("Discharging animation...");
                while (percentage > 1)
                {
                    percentage -= 0.10m;
                    Console.WriteLine($"{percentage}");
                    ledBarGraph.Percentage = (float)Math.Max(0.0m, percentage);
                    ledBarGraph.SetLedBlink(ledBarGraph.GetTopLedForPercentage());
                    await Task.Delay(1000);
                }

                await Task.Delay(1000);

                Console.WriteLine("Blinking for 3 seconds...");
                ledBarGraph.StartBlink();
                await Task.Delay(5000);
                ledBarGraph.Stop();

                await Task.Delay(1000);

                Console.WriteLine("Blinking for 3 seconds...");
                ledBarGraph.StartBlink(TimeSpan.FromSeconds(500), TimeSpan.FromSeconds(500));
                await Task.Delay(5000);
                ledBarGraph.Stop();

                await Task.Delay(1000);

                for (int i = 0; i < ledBarGraph.Count; i++)
                {
                    ledBarGraph.SetLed(i, false);
                }
            }
        }

        //<!=SNOP=>
    }
}