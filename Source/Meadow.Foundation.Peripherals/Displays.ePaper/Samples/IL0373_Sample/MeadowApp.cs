using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using System.Threading.Tasks;

namespace Displays.ePaper.IL0373_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initialize ...");

            var display = new Il0373(
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D03,
                dcPin: Device.Pins.D02,
                resetPin: Device.Pins.D01,
                busyPin: Device.Pins.D00,
                width: 400,
                height: 300);

            graphics = new MicroGraphics(display);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            //any color but black will show the ePaper alternate color 
            graphics.DrawRectangle(1, 1, 126, 32, Meadow.Foundation.Color.Black, false);

            graphics.CurrentFont = new Font12x16();
            graphics.DrawText(2, 2, "IL0373", Meadow.Foundation.Color.Black);
            graphics.DrawText(2, 30, "Meadow F7", Meadow.Foundation.Color.Black);

            graphics.Show();

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}