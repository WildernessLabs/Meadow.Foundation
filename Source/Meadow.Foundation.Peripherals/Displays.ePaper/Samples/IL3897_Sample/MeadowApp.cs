using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.ePaper;
using Meadow.Foundation.Graphics;

namespace Displays.ePaper.IL3897_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        //<!—SNIP—>

        public MeadowApp()
        {
            Console.WriteLine("Initialize ...");
 
            var display = new Il3897(device: Device,
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                busyPin: Device.Pins.D03,
                width: 122,
                height: 250);

            var graphics = new MicroGraphics(display);

            graphics.DrawRectangle(1, 1, 126, 32, Meadow.Foundation.Color.Black, false);

            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(2, 2, "IL3897", Meadow.Foundation.Color.Black);
            graphics.DrawText(2, 20, "Meadow F7", Meadow.Foundation.Color.Black);

            graphics.Show();
        }

        //<!—SNOP—>
    }
}