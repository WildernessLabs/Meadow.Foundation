using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using System.Threading.Tasks;

namespace SharpMemoryDisplay_Sample
{
    public class MeadowApp : App<F7FeatherV1>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            var display = new SharpMemoryDisplay(
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D00,
                width: 144,
                height: 168);

            graphics = new MicroGraphics(display)
            {
                CurrentFont = new Font8x12(),
                PenColor = Color.Black
            };

            return base.Initialize();
        }

        public override Task Run()
        {
            graphics.Clear();
            graphics.DrawText(0, 0, "Sharp Memory");
            graphics.DrawText(0, 14, "144x168");
            graphics.DrawRectangle(0, 24, 108, 108);
            graphics.DrawCircle(60, 100, 40);
            graphics.Show();

            return base.Run();
        }

        //<!=SNOP=>
    }
}
