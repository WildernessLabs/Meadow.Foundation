using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.mikroBUS.Displays;
using Meadow.Peripherals.Displays;
using System;

namespace C16x9_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        readonly C16x9 c16x9;

        public MeadowApp()
        {
            Console.WriteLine("Initializing ...");

            c16x9 = new C16x9(Device.Pins.D14, Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Standard));
            c16x9.IgnoreOutOfBoundsPixels = true;

            c16x9.Clear();

            var graphics = new MicroGraphics(c16x9)
            {
                Rotation = RotationType._180Degrees
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