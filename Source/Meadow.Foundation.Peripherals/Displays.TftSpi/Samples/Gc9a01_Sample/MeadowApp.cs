using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using System.Threading.Tasks;

namespace Displays.Tft.Gc9a01_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing ...");

            var spiBus = Device.CreateSpiBus();

            Resolver.Log.Info("Create display driver instance");

            var display = new Gc9a01
            (
                spiBus: spiBus,
                chipSelectPin: Device.Pins.A02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00
            );

            graphics = new MicroGraphics(display)
            {
                IgnoreOutOfBoundsPixels = true,
                CurrentFont = new Font12x20(),
                Rotation = RotationType._180Degrees
            };

            return base.Initialize();
        }

        public override Task Run()
        {
            graphics.Clear();
            graphics.DrawCircle(120, 120, 100, Color.Cyan, false);
            graphics.DrawRoundedRectangle(50, 50, 140, 140, 50, Color.BlueViolet, false);
            graphics.DrawText(120, 120, "Meadow F7", alignmentH: HorizontalAlignment.Center, alignmentV: VerticalAlignment.Center);
            graphics.Show();

            return base.Run();
        }

        //<!=SNOP=>
    }
}