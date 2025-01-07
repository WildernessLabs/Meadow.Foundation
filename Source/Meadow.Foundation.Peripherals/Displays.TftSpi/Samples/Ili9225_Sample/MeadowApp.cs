using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using System.Threading.Tasks;

namespace Displays.Tft.Ili9225_Sample
{
    public class MeadowApp : App<F7FeatherV1>
    {
        //<!=SNIP=>

        MicroGraphics graphics;
        Ili9225 display;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing ...");

            display = new Ili9225
            (
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D00,
                resetPin: Device.Pins.D01
            );

            graphics = new MicroGraphics(display)
            {
                IgnoreOutOfBoundsPixels = true,
                CurrentFont = new Font12x16()
            };

            return base.Initialize();
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run ...");

            graphics.Clear();

            graphics.DrawTriangle(10, 30, 50, 50, 10, 50, Color.Red);
            graphics.DrawRectangle(20, 45, 40, 20, Color.Yellow, false);
            graphics.DrawCircle(50, 50, 40, Color.Blue, false);
            graphics.DrawText(5, 5, "Meadow F7", Color.White);

            graphics.Show();

            Resolver.Log.Info("Complete ...");

            return base.Run();
        }

        //<!=SNOP=>
    }
}