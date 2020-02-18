using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.ePaper;
using Meadow.Foundation.Graphics;

namespace Displays.ePaper.IL91874_Sample
{
    /* Driver in development */
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Il91874 display;

        public MeadowApp()
        {
            Console.WriteLine("ePaper sample");
            Console.WriteLine("Create Spi bus");

            var spiBus = Device.CreateSpiBus();// Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, 2000);

            Console.WriteLine("Create display driver instance");
            display = new Il91874(device: Device, spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                busyPin: Device.Pins.D03,
                width: 176,
                height: 264);

            var graphics = new GraphicsLibrary(display);

            //any color but black will show the ePaper alternate color 
            graphics.DrawRectangle(1, 1, 126, 32, Meadow.Foundation.Color.Red, false);

            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(2, 2, "IL91874");
            graphics.DrawText(2, 20, "Meadow F7");

            int ySpacing = 6;

            for (int i = 0; i < 3; i++)
            {
                graphics.DrawLine(2, 70 + ySpacing * i, 22, 50 + ySpacing * i, true);
                graphics.DrawLine(22, 50 + ySpacing * i, 42, 70 + ySpacing * i, true);
                graphics.DrawLine(44, 70 + ySpacing * i, 64, 50 + ySpacing * i, true);
                graphics.DrawLine(64, 50 + ySpacing * i, 84, 70 + ySpacing * i, true);
                graphics.DrawLine(86, 70 + ySpacing * i, 106, 50 + ySpacing * i, true);
                graphics.DrawLine(106, 50 + ySpacing * i, 126, 70 + ySpacing * i, true);
            }

            Console.WriteLine("Show");

            graphics.Show();
        }
    }
}