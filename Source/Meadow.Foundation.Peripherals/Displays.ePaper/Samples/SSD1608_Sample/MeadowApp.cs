using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.ePaper;
using Meadow.Foundation.Graphics;

namespace Displays.ePaper.SSD1608_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        public MeadowApp()
        {
            Console.WriteLine("Initialize ...");
 
            var display = new Ssd1608(device: Device,
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                busyPin: Device.Pins.D03,
                width: 200,
                height: 200);

            var graphics = new MicroGraphics(display);

           graphics.DrawRectangle(1, 1, 126, 32, Meadow.Foundation.Color.Black);

            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(2, 2, "SSD1608", Meadow.Foundation.Color.Black);
            graphics.DrawText(2, 20, "Meadow F7", Meadow.Foundation.Color.Black);

            graphics.Show();
        }

        //<!—SNOP—>
    }
}