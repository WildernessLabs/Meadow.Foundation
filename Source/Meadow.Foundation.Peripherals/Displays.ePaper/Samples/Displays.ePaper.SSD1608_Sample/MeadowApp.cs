using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.ePaper;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace Displays.ePaper.SSD1608_Sample
{
    /* Driver in development */
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Ssd1608 display;

        public MeadowApp()
        {
            Console.WriteLine("ePaper sample");
            Console.WriteLine("Create Spi bus");

            var spiBus = Device.CreateSpiBus();// Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, 2000);

            Console.WriteLine("Create display driver instance");
            display = new Ssd1608(device: Device, spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                busyPin: Device.Pins.D03,
                width: 200,
                height: 200);

            var graphics = new GraphicsLibrary(display);

            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(2, 2, "SSD1608");
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