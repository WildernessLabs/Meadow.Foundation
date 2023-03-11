using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using System;
using System.Threading.Tasks;

namespace Displays.Pcd8854_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initializing...");

            var config = new Meadow.Hardware.SpiClockConfiguration(Pcd8544.DEFAULT_SPEED, Meadow.Hardware.SpiClockConfiguration.Mode.Mode0);

            var display = new Pcd8544
            (
                spiBus: Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config),
                chipSelectPin: Device.Pins.D01,
                dcPin: Device.Pins.D00,
                resetPin: Device.Pins.D02
            );

            graphics = new MicroGraphics(display);

            return base.Initialize(args);
        }

        public override Task Run()
        {
            graphics.Clear(true);
            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(0, 0, "PCD8544");
            graphics.DrawRectangle(5, 14, 30, 10, true);

            graphics.Show();

            return base.Run();
        }

        //<!=SNOP=>

        void CounterDemo(MicroGraphics graphics)
        {
            int count = 0;

            graphics.CurrentFont = new Font12x20();

            while(true)
            {
                graphics.Clear();
                graphics.DrawText(0, 0, $"Count:");
                graphics.DrawText(0, 24, $"{count}");
                graphics.Show();
                count++;
            }
        }
    }
}