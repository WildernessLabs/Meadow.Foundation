using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace Displays.Tft.Ili9481_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        MicroGraphics graphics;

        public MeadowApp()
        {
            Console.WriteLine("Initializing ...");

            var config = new SpiClockConfiguration(12000, SpiClockConfiguration.Mode.Mode0);
            var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            Console.WriteLine("Create display driver instance");

            var display = new Ili9481
            (
                device: Device, 
                spiBus: spiBus,
                resetPin: Device.Pins.D00,
				chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                width: 320, height: 480
            )
            {
                IgnoreOutOfBoundsPixels = true
            };

            graphics = new MicroGraphics(display);

            graphics.CurrentFont = new Font8x8();
            graphics.Clear();
            graphics.DrawTriangle(10, 10, 50, 50, 10, 50, Meadow.Foundation.Color.Red);
            graphics.DrawRectangle(20, 15, 40, 20, Meadow.Foundation.Color.Yellow, false);
            graphics.DrawCircle(50, 50, 40, Meadow.Foundation.Color.Blue, false);
            graphics.DrawText(5, 5, "Meadow F7");
            graphics.Show();
        }

        //<!—SNOP—>
    }
}