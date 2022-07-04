using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;

namespace BasicDisplays.Tft.Ssd1351_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        public MeadowApp()
        {
            Console.WriteLine("Initializing ...");
  
            var spiBus = Device.CreateSpiBus(Ssd1351.DefaultSpiBusSpeed);

            var display = new Ssd1351(
                       device: Device, 
                       spiBus: spiBus,
                       chipSelectPin: Device.Pins.D02,
                       dcPin: Device.Pins.D01,
                       resetPin: Device.Pins.D00,
                       width: 128, height: 128)
            {
            };

            var graphics = new MicroGraphics(display)
            {
                CurrentFont = new Font8x12(),
                IgnoreOutOfBoundsPixels = true
            };

            graphics.Clear();

            graphics.DrawCircle(80, 80, 40, Meadow.Foundation.Color.Cyan, false);

            int indent = 0;
            int spacing = 10;
            int y = indent;

            graphics.DrawText(indent, y, "Meadow F7 (SSD1351)");

            graphics.DrawText(indent, y += spacing, "Red", Meadow.Foundation.Color.Red);
            graphics.DrawText(indent, y += spacing, "Purple", Meadow.Foundation.Color.Purple);
            graphics.DrawText(indent, y += spacing, "BlueViolet", Meadow.Foundation.Color.BlueViolet);
            graphics.DrawText(indent, y += spacing, "Blue", Meadow.Foundation.Color.Blue);
            graphics.DrawText(indent, y += spacing, "Cyan", Meadow.Foundation.Color.Cyan);
            graphics.DrawText(indent, y += spacing, "LawnGreen", Meadow.Foundation.Color.LawnGreen);
            graphics.DrawText(indent, y += spacing, "GreenYellow", Meadow.Foundation.Color.GreenYellow);
            graphics.DrawText(indent, y += spacing, "Yellow", Meadow.Foundation.Color.Yellow);
            graphics.DrawText(indent, y += spacing, "Orange", Meadow.Foundation.Color.Orange);
            graphics.DrawText(indent, y += spacing, "Brown", Meadow.Foundation.Color.Brown);

            graphics.Show();
        }

        //<!=SNOP=>
    }
}