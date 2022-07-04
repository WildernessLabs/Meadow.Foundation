using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using System;
using Meadow.Units;
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
            pwmLedBarGraph = new PwmLedBarGraph(Device, pins, new Voltage(3.3));

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Console.WriteLine("TestLedBarGraph...");

            double percentage = 0;

            while (true)
            {
                Console.WriteLine("Turning them on using SetLed...");
                for (int i = 0; i < pwmLedBarGraph.Count; i++)
                {
                    pwmLedBarGraph.SetLed(i, true);
                    await Task.Delay(300);
                }

                await Task.Delay(1000);

                Console.WriteLine("Turning them off using SetLed...");
                for (int i = pwmLedBarGraph.Count - 1; i >= 0; i--)
                {
                    pwmLedBarGraph.SetLed(i, false);
                    await Task.Delay(300);
                }

                await Task.Delay(1000);

                Console.WriteLine("Turning them on using Percentage...");
                while (percentage < 1)
                {
                    percentage += 0.01;
                    pwmLedBarGraph.Percentage = (float) Math.Min(1.0, percentage);
                    await Task.Delay(100);
                }

                await Task.Delay(1000);

                Console.WriteLine("Turning them off using Percentage...");
                while (percentage > 0)
                {
                    percentage -= 0.01;
                    pwmLedBarGraph.Percentage = (float)Math.Max(0.0, percentage);
                    await Task.Delay(100);
                }

                await Task.Delay(1000);

                Console.WriteLine("Bar blinking on and off...");
                pwmLedBarGraph.StartBlink();
                await Task.Delay(3000);
                pwmLedBarGraph.Stop();

                await Task.Delay(1000);

                Console.WriteLine("Bar blinking with high and low brightness...");
                pwmLedBarGraph.StartBlink(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500), 1f, 0.25f);
                await Task.Delay(3000);
                pwmLedBarGraph.Stop();

                await Task.Delay(1000);

                Console.WriteLine("Bar pulsing...");
                pwmLedBarGraph.StartPulse();
                await Task.Delay(3000);
                pwmLedBarGraph.Stop();

                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}