using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using System.Threading.Tasks;

namespace Displays.ePaper.EpdColor_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize ...");

            var display = new Epd7in5V2(
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D03,
                dcPin: Device.Pins.D02,
                resetPin: Device.Pins.D01,
                busyPin: Device.Pins.D00);

            graphics = new MicroGraphics(display);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run");

            graphics.Clear();

            graphics.CurrentFont = new Font12x16();
            graphics.DrawText(0, 0, "Meadow F7", Color.Black, scaleFactor: ScaleFactor.X2);
            graphics.DrawText(0, 50, "Green", Color.Green, scaleFactor: ScaleFactor.X2);
            graphics.DrawText(0, 100, "Yellow", Color.Yellow, scaleFactor: ScaleFactor.X2);
            graphics.DrawText(0, 150, "Orange", Color.Orange, scaleFactor: ScaleFactor.X2);
            graphics.DrawText(0, 200, "Red", Color.Red, scaleFactor: ScaleFactor.X2);
            graphics.DrawText(0, 250, "Blue", Color.Blue, scaleFactor: ScaleFactor.X2);

            graphics.Show();

            Resolver.Log.Info("Run complete");

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}