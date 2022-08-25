using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.ePaper;
using Meadow.Foundation.Graphics;

namespace Displays.ePaper.IL0398_Sample
{
    public class MeadowApp : App<F7FeatherV1>
    {
        //<!=SNIP=>

        Il0398 display;
        MicroGraphics graphics;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize ...");
 
            display = new Il0398(device: Device,
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D03,
                dcPin: Device.Pins.D02,
                resetPin: Device.Pins.D01,
                busyPin: Device.Pins.D00);

            graphics = new MicroGraphics(display);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Console.WriteLine("Run");

            //any color but black will show the ePaper alternate color 
            //graphics.DrawRectangle(1, 1, 126, 32, Color.Red, false);

            graphics.CurrentFont = new Font8x12();
            //graphics.DrawText(2, 2, "IL0398", Color.Black);
            //graphics.DrawText(2, 20, "Meadow F7", Color.Black);

            // graphics.DrawRectangle(10, 20, 30, 40, false, false);

            for (int i = 0; i < 100; i++)
            {
                // display.DrawPixel(i, i, true);
                graphics.DrawPixel(i, i, true);
            }

            graphics.DrawRectangle(10, 20, 30, 40, true, false);

            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(2, 20, "Meadow F7", Color.White);

            graphics.Show();

            Console.WriteLine("Run complete");

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}