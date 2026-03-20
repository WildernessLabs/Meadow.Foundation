using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.mikroBUS.Displays;
using Meadow.Peripherals.Displays;
using System;

namespace C8x8_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        readonly C8x8 c8x8;

        public MeadowApp()
        {
            Console.WriteLine("Initializing ...");

            c8x8 = new C8x8(Device.CreateSpiBus(), Device.Pins.D14);

            var graphics = new MicroGraphics(c8x8)
            {
                IgnoreOutOfBoundsPixels = true,
                Rotation = RotationType._270Degrees
            };
            graphics.CurrentFont = new Font4x8();

            var message = "Wilderness Labs Meadow F7 Feather";

            while (true)
            {
                for (int x = 0; x < message.Length * 4; x++)
                {
                    graphics.Clear();
                    graphics.DrawText(0 - x, 1, message);
                    graphics.Show();
                }
            }
        }

        //<!=SNOP=>
    }
}