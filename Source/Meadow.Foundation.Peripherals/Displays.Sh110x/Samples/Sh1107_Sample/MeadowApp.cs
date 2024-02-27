using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using System.Threading.Tasks;
using static Meadow.Foundation.Displays.Sh110x;

namespace Displays.Sh1107_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            var sh1107 = new Sh1107
            (
                i2cBus: Device.CreateI2cBus(),
                address: (byte)Addresses.Address_0x3C,
                width: 128,
                height: 128
            );

            graphics = new MicroGraphics(sh1107)
            {
                CurrentFont = new Font12x16(),
                Rotation = RotationType._180Degrees
            };

            return base.Initialize();
        }

        public override Task Run()
        {
            graphics.Clear();
            graphics.DrawRectangle(0, 0, graphics.Width, graphics.Height, false);
            graphics.DrawTriangle(10, 10, 50, 50, 10, 50, false);
            graphics.DrawRectangle(20, 15, 40, 20, true);
            graphics.DrawText(5, 5, "SH1107");
            graphics.Show();

            return base.Run();
        }

        //<!=SNOP=>
    }
}