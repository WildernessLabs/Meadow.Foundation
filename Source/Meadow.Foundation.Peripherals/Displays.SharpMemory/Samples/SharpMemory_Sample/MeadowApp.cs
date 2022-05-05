using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace Displays.SharpMemory_Sample
{
    public class MeadowApp : App<F7FeatherV1, MeadowApp>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            var display = new SharpMemory
            (
                device: Device, 
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D00
            );

            graphics = new MicroGraphics(display);

            graphics.CurrentFont = new Font8x8();
            graphics.Clear();
            graphics.DrawTriangle(10, 10, 50, 50, 10, 50);
            graphics.DrawRectangle(20, 15, 40, 20, true);            
            graphics.DrawText(5, 5, "Sharp");
            graphics.Show();

            Console.WriteLine("Done");
        }

        //<!=SNOP=>
    }
}