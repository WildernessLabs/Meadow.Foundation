using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Leds.PwmLedBarGraph_Sample
{
    public class MeadowApp : App<F7FeatherV1>
    {
        //<!=SNIP=>

        PwmLedBarGraph pwmLedBarGraph;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

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

            pwmLedBarGraph = new PwmLedBarGraph(pins, new Voltage(2.2));

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info("TestLedBarGraph...");

            float percentage = 0;

            while (true)
            {
                Resolver.Log.Info("Turning them on and off for 200ms using SetLed...");
                for (int i = 0; i < pwmLedBarGraph.Count; i++)
                {
                    await pwmLedBarGraph.SetLed(i, true);
                    await Task.Delay(100);
                    await pwmLedBarGraph.SetLed(i, false);
                }

                await Task.Delay(1000);

                Resolver.Log.Info("Turning them on using Percentage...");
                while (percentage < 1)
                {
                    percentage += 0.01f;
                    await pwmLedBarGraph.SetPercentage(Math.Min(1.0f, percentage));
                    await Task.Delay(100);
                }

                await Task.Delay(1000);

                Resolver.Log.Info("Turning them off using Percentage...");
                while (percentage > 0)
                {
                    percentage -= 0.01f;
                    await pwmLedBarGraph.SetPercentage(Math.Max(0.0f, percentage));
                    await Task.Delay(100);
                }

                await Task.Delay(1000);

                Resolver.Log.Info("Blinking for 5 seconds at 500ms on/off...");
                await pwmLedBarGraph.StartBlink();
                await Task.Delay(5000);
                await pwmLedBarGraph.StopAnimation();

                await Task.Delay(1000);

                Resolver.Log.Info("Bar blinking with high and low brightness for 5 seconds...");
                await pwmLedBarGraph.StartBlink(TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(200), 0.75f, 0.25f);
                await Task.Delay(5000);
                await pwmLedBarGraph.StopAnimation();

                await Task.Delay(1000);

                Resolver.Log.Info("Bar pulsing for 5 seconds...");
                await pwmLedBarGraph.StartPulse();
                await Task.Delay(5000);
                await pwmLedBarGraph.StopAnimation();

                await Task.Delay(1000);

                await pwmLedBarGraph.SetBrightness(0);

                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}