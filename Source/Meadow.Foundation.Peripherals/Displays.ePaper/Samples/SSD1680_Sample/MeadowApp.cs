using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using System.Threading.Tasks;

namespace Displays.ePaper.SSD1680_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize ...");

            var display = new Ssd1680(
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.A04,
                dcPin: Device.Pins.A03,
                resetPin: Device.Pins.A02,
                busyPin: Device.Pins.A01,
                width: 122,
                height: 250);

            graphics = new MicroGraphics(display)
            {
                Rotation = RotationType._270Degrees
            };

            return base.Initialize();
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run ...");

            graphics.Clear();

            graphics.DrawRectangle(10, 40, 120, 60, Color.Black, true);
            graphics.DrawRectangle(20, 80, 120, 90, Color.Red, true);

            graphics.CurrentFont = new Font12x16();
            graphics.DrawText(2, 20, "Meadow F7", Color.Black);
            graphics.DrawText(30, 50, "Color", Color.Red);
            graphics.DrawText(50, 90, "Black", Color.Black);
            graphics.DrawText(50, 120, "White", Color.White);

            graphics.Show();

            return base.Run();
        }

        //<!=SNOP=>
    }
}