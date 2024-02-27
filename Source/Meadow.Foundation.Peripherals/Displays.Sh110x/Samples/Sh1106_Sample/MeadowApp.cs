using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using System.Threading.Tasks;

namespace Displays.Sh1106_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            var sh1106 = new Sh1106
            (
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00
            );

            graphics = new MicroGraphics(sh1106);
            graphics.CurrentFont = new Font8x8();
            graphics.Rotation = RotationType._180Degrees;

            return base.Initialize();
        }

        public override Task Run()
        {
            graphics.Clear();
            graphics.DrawRectangle(0, 0, 128, 64, false);
            graphics.DrawTriangle(10, 10, 50, 50, 10, 50, false);
            graphics.DrawRectangle(20, 15, 40, 20, true);
            graphics.DrawText(5, 5, "SH1106");
            graphics.Show();

            return base.Run();
        }

        //<!=SNOP=>
    }
}