using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Tft;
using Meadow.Foundation.Graphics;

namespace Displays.Tft.ILI9163_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        ILI9163 display;
        GraphicsLibrary graphics;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            var spiBus = Device.CreateSpiBus();

            display = new ILI9163
            (
                device: Device, 
                spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 128, height: 160
            );

            graphics = new GraphicsLibrary(display);

            TestILI9163();
        }

        void TestILI9163() 
        {
            Console.WriteLine("Clear display");

            // Drawing natively in the display
            display.ClearScreen(250);

            Console.WriteLine("Refresh");

            display.Refresh();

            Console.WriteLine("Draw");

            for (int i = 0; i < 30; i++)
            {
                display.DrawPixel(i, i, true);
                display.DrawPixel(30 + i, i, true);
                display.DrawPixel(60 + i, i, true);
            }

            Console.WriteLine("Show");

            display.Show();

            Console.WriteLine("Show complete");

            // Drawing with Display Graphics Library
            graphics.CurrentFont = new Font8x8();
            graphics.Clear();
            graphics.DrawTriangle(10, 10, 50, 50, 10, 50, Meadow.Foundation.Color.Red);
            graphics.DrawRectangle(20, 15, 40, 20, Meadow.Foundation.Color.Yellow, false);
            graphics.DrawCircle(50, 50, 40, Meadow.Foundation.Color.Blue, false);
            graphics.DrawText(5, 5, "Meadow F7 SPI");
            graphics.Show();
        }
    }
}