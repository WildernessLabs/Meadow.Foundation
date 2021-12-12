using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.ePaper;
using Meadow.Foundation.Graphics;

namespace Displays.ePaper.IL91874_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>
        
        public MeadowApp()
        {
            Console.WriteLine("Initialize ...");
 
            var display = new Il91874(device: Device,
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                busyPin: Device.Pins.D03,
                width: 176,
                height: 264);

            var graphics = new MicroGraphics(display);

            //any color but black will show the ePaper alternate color 
            graphics.DrawRectangle(1, 1, 126, 32, Meadow.Foundation.Color.Red, false);

            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(2, 2, "IL91874", Meadow.Foundation.Color.Black);
            graphics.DrawText(2, 20, "Hello, Honeybees!", Meadow.Foundation.Color.Black);

            graphics.Show();
        }

        //<!—SNOP—>
    }
}