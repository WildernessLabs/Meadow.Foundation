using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using System;
using System.Threading;

namespace BasicPCD8843Display_Sample
{
    public class PCD8854DisplayApp : App<F7Micro, PCD8854DisplayApp>
    {

        PCD8544 display;
        GraphicsLibrary graphics;

        public PCD8854DisplayApp()
        {
            var spiBus = Device.CreateSpiBus();

            display = new PCD8544
            (
                device: Device, 
                spiBus: spiBus,
                chipSelectPin: Device.Pins.D01, 
                dcPin: Device.Pins.D00, 
                resetPin: Device.Pins.D02
            );
            Console.WriteLine("1");

            for (int i = 0; i < 35; i++)
            {
                display.DrawPixel(i, i, true);
            }

            Console.WriteLine("2");

            graphics = new GraphicsLibrary(display);

            Console.WriteLine("3");

            graphics.Clear(true);
            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(0, 0, "Meadow F7");
            graphics.DrawRectangle(5, 14, 30, 10, true);

            Console.WriteLine("4");

            Thread.Sleep(-1);

            /*
            graphics = new Meadow.Foundation.Graphics.GraphicsLibrary(display);

            graphics.Clear(true);

            graphics.DrawRectangle(2, 2, 20, 20, false);

            graphics.DrawCircle(20, 30, 12, false);

    */


        }
    }
}
