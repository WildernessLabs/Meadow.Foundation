using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Leds.PwmLedBarGraph_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        PwmLedBarGraph pwmLedBarGraph;

        public override Task Initialize()
        {
            Console.WriteLine("Initializing...");

            // Using an array of Pins that support PWM (D02 - D13)
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

            pwmLedBarGraph = new PwmLedBarGraph(Device, pins, new Voltage(2.2));

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Console.WriteLine("TestLedBarGraph...");

            float percentage = 0;

            while (true)
            {
                Console.WriteLine("Turning them on and off for 200ms using SetLed...");
                for (int i = 0; i < pwmLedBarGraph.Count; i++)
                {
                    pwmLedBarGraph.SetLed(i, true);
                    await Task.Delay(100);
                    pwmLedBarGraph.SetLed(i, false);
                }

                await Task.Delay(1000);

                Console.WriteLine("Turning them on using Percentage...");
                while (percentage < 1)
                {
                    percentage += 0.01f;
                    pwmLedBarGraph.Percentage = Math.Min(1.0f, percentage);
                    await Task.Delay(100);
                }

                await Task.Delay(1000);

                Console.WriteLine("Turning them off using Percentage...");
                while (percentage > 0)
                {
                    percentage -= 0.01f;
                    pwmLedBarGraph.Percentage = Math.Max(0.0f, percentage);
                    await Task.Delay(100);
                }

                await Task.Delay(1000);

                Console.WriteLine("Blinking for 5 seconds at 500ms on/off...");
                pwmLedBarGraph.StartBlink();
                await Task.Delay(5000);
                pwmLedBarGraph.Stop();

                await Task.Delay(1000);

                Console.WriteLine("Bar blinking with high and low brightness for 5 seconds...");
                pwmLedBarGraph.StartBlink(TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(200), 0.75f, 0.25f);
                await Task.Delay(5000);
                pwmLedBarGraph.Stop();

                await Task.Delay(1000);

                Console.WriteLine("Bar pulsing for 5 seconds...");
                pwmLedBarGraph.StartPulse();
                await Task.Delay(5000);
                pwmLedBarGraph.Stop();

                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}