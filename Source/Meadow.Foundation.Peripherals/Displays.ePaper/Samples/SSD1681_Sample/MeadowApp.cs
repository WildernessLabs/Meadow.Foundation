using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.ePaper;
using Meadow.Foundation.Graphics;

namespace Displays.ePaper.SSD1681_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize ...");
 
            var display = new Ssd1681(device: Device,
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                busyPin: Device.Pins.D03,
                width: 200,
                height: 200);

            graphics = new MicroGraphics(display);

            return base.Initialize();
        }

        public override Task Run()
        {
            graphics.DrawRectangle(1, 1, 126, 32, Meadow.Foundation.Color.Black);

            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(2, 2, "SSD1681", Meadow.Foundation.Color.Black);
            graphics.DrawText(2, 20, "Meadow F7", Meadow.Foundation.Color.Black);

            graphics.Show();

            return base.Run();
        }

        //<!=SNOP=>
    }
}