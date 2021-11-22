using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;

namespace Displays.TftSpi.St7735_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        //<!—SNIP—>

        public MeadowApp()
        {
            Console.WriteLine("Initializing ...");

            var spiBus = Device.CreateSpiBus(12000);

            //note - you may need to adjust the DisplayType for your specific St7735
            var display = new St7735(
                device: Device, 
                spiBus: Device.CreateSpiBus(),
              chipSelectPin: Device.Pins.D02,
              dcPin: Device.Pins.D01,
              resetPin: Device.Pins.D00,
              width: 128, height: 160, St7735.DisplayType.ST7735R);

            var graphics = new MicroGraphics(display);

            graphics.Clear();

            graphics.DrawCircle(60, 60, 20, Color.Purple);
            graphics.DrawRectangle(10, 10, 30, 60, Color.Red);
            graphics.DrawTriangle(20, 20, 10, 70, 60, 60, Color.Green);

            graphics.DrawCircle(90, 60, 20, Color.Cyan, true);
            graphics.DrawRectangle(100, 100, 30, 10, Color.Yellow, true);
            graphics.DrawTriangle(120, 20, 110, 70, 160, 60, Color.Pink, true);

            graphics.DrawLine(10, 120, 110, 130, Color.SlateGray);

            graphics.Show();
        }

        //<!—SNOP—>
    }
}