using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using System.Threading.Tasks;

namespace Displays.ePaper.Uc8151c_Sample
{
    public class MeadowApp : App<F7FeatherV1>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initialize ...");

            var display = new Uc8151c(
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D03,
                dcPin: Device.Pins.D02,
                resetPin: Device.Pins.D01,
                busyPin: Device.Pins.D00,
                width: 152,
                height: 152);

            graphics = new MicroGraphics(display);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run");

            for (int i = 0; i < 100; i++)
            {
                graphics.DrawPixel(i, i, Color.Black);
            }

            graphics.DrawRectangle(10, 40, 120, 60, Color.Black, true);
            graphics.DrawRectangle(20, 80, 120, 90, Color.Red, true);

            graphics.CurrentFont = new Font12x16();
            graphics.DrawText(2, 20, "Meadow F7", Color.Black);
            graphics.DrawText(30, 50, "Color", Color.Red);
            graphics.DrawText(50, 90, "Black", Color.Black);
            graphics.DrawText(50, 120, "White", Color.White);

            graphics.Show();

            Resolver.Log.Info("Run complete");

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}