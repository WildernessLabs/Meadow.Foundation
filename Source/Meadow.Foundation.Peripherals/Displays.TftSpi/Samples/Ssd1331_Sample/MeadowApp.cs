using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace Displays.Tft.Ssd1331_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        MicroGraphics graphics;

        public MeadowApp()
        {
            Console.WriteLine("Initializing ...");

            var config = new SpiClockConfiguration(new Meadow.Units.Frequency(12000, Meadow.Units.Frequency.UnitType.Kilohertz),
                SpiClockConfiguration.Mode.Mode0);
            var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            Console.WriteLine("Create display driver instance");

            var display = new St7796s
            (
                device: Device, 
                spiBus: spiBus,
                resetPin: Device.Pins.D00,
				chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                width: 320, height: 480
            );

            graphics = new MicroGraphics(display);

            graphics.CurrentFont = new Font8x8();
            graphics.Clear();
            graphics.DrawTriangle(10, 10, 50, 50, 10, 50, Meadow.Foundation.Color.Red);
            graphics.DrawRectangle(20, 15, 40, 20, Meadow.Foundation.Color.Yellow, false);            
            graphics.DrawText(5, 5, "Meadow F7");
            graphics.Show();
        }

        //<!—SNOP—>
    }
}