using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using System.Threading.Tasks;

namespace Displays.ePaper.Epd7in5V2_Sample
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
                busyPin: Device.Pins.D00,
                ColorMode.Format2bppGray);

            graphics = new MicroGraphics(display);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run");

            graphics.Clear();

            graphics.CurrentFont = new Font12x16();
            graphics.DrawText(0, 0, "Meadow F7", Color.Black, scaleFactor: ScaleFactor.X2);
            graphics.DrawText(0, 50, $"Black ({Color.Black.R}, {Color.Black.G}, {Color.Black.B})", Color.Black, scaleFactor: ScaleFactor.X2);
            graphics.DrawText(0, 100, $"Slate Gray ({Color.SlateGray.R}, {Color.SlateGray.G}, {Color.SlateGray.B})", Color.SlateGray, scaleFactor: ScaleFactor.X2);
            graphics.DrawText(0, 150, $"Dim Gray ({Color.DimGray.R}, {Color.DimGray.G}, {Color.DimGray.B})", Color.DimGray, scaleFactor: ScaleFactor.X2);
            graphics.DrawText(0, 200, $"Dark Gray ({Color.DarkGray.R}, {Color.DarkGray.G}, {Color.DarkGray.B})", Color.DarkGray, scaleFactor: ScaleFactor.X2);

            graphics.DrawRectangle(0, 240, graphics.Width, graphics.Height - 240, Color.SlateGray, true);

            graphics.DrawText(0, 250, $"Dark Gray ({Color.DarkGray.R}, {Color.DarkGray.G}, {Color.DarkGray.B})", Color.DarkGray, scaleFactor: ScaleFactor.X2);
            graphics.DrawText(0, 300, $"Light Gray ({Color.LightGray.R}, {Color.LightGray.G}, {Color.LightGray.B})", Color.LightGray, scaleFactor: ScaleFactor.X2);
            graphics.DrawText(0, 350, $"White ({Color.White.R}, {Color.White.G}, {Color.White.B})", Color.White, scaleFactor: ScaleFactor.X2);

            graphics.Show();

            Resolver.Log.Info("Run complete");

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}