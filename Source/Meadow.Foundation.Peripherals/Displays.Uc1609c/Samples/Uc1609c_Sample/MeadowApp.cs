using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using System.Threading.Tasks;

namespace Displays.Uc1609c_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            var uc1609c = new Uc1609c
            (
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.A03,
                dcPin: Device.Pins.A04,
                resetPin: Device.Pins.A05,
                width: 192,
                height: 64
            );

            graphics = new MicroGraphics(uc1609c)
            {
                CurrentFont = new Font8x8()
            };

            return base.Initialize();
        }

        public override Task Run()
        {
            graphics.Clear();
            graphics.DrawTriangle(10, 10, 50, 50, 10, 50, false);
            graphics.DrawRectangle(20, 15, 40, 20, true);
            graphics.DrawText(5, 5, "UC1609C");
            graphics.DrawCircle(96, 32, 16);
            graphics.Show();

            Resolver.Log.Info("Run complete");

            return base.Run();
        }

        //<!=SNOP=>
    }
}