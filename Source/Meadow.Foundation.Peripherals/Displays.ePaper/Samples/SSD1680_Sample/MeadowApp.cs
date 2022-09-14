using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;

namespace Displays.ePaper.SSD1680_Sample
{
    public class MeadowApp : App<F7FeatherV1>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize ...");
 
            var display = new Ssd1680(device: Device,
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D03,
                dcPin: Device.Pins.D02,
                resetPin: Device.Pins.D01,
                busyPin: Device.Pins.D00,
                width: 250,
                height: 128);

            graphics = new MicroGraphics(display);

            return base.Initialize();
        }

        public override Task Run()
        {
            graphics.Clear();

            for(int i = 0; i < 20; i++)
            {
                graphics.DrawPixel(i, i, Meadow.Foundation.Color.Black);
            }

            /*

            graphics.DrawRectangle(1, 1, 126, 32, Meadow.Foundation.Color.Black);

            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(2, 2, "SSD1680", Meadow.Foundation.Color.Black);
            graphics.DrawText(2, 20, "Meadow F7", Meadow.Foundation.Color.Black);*/

            graphics.Show();

            return base.Run();
        }

        //<!=SNOP=>
    }
}