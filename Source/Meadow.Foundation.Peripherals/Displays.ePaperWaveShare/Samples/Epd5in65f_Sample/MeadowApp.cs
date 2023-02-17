using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
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

            var display = new Epd5in65f(
                    spiBus: Device.CreateSpiBus(),
                    chipSelectPin: Device.Pins.A04,
                    dcPin: Device.Pins.A03,
                    resetPin: Device.Pins.A02,
                    busyPin: Device.Pins.A01);

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