using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Tft;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace BasicDisplays.Tft.SSD1351_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        DisplayTftSpiBase display;
        ISpiBus spiBus;

        public MeadowApp()
        {
            Console.WriteLine("TftSpi sample");
            Console.WriteLine("Create Spi bus");

            spiBus = Device.CreateSpiBus(3000);

            Console.WriteLine("Create display driver instance");

            display = new SSD1351(device: Device, spiBus: spiBus,
                       chipSelectPin: Device.Pins.D02,
                       dcPin: Device.Pins.D01,
                       resetPin: Device.Pins.D00,
                       width: 128, height: 128); 

            Console.WriteLine("Create graphics lib");

            var graphicsLib = new GraphicsLibrary(display);
            graphicsLib.CurrentFont = new Font8x12();

            graphicsLib.Clear();

            graphicsLib.DrawCircle(80, 80, 40, Meadow.Foundation.Color.Cyan, false);

            int indent = 0;
            int spacing = 10;
            int y = indent;

            graphicsLib.DrawText(indent, y, "Meadow F7 (SSD1351)");

            graphicsLib.DrawText(indent, y += spacing, "Red", Meadow.Foundation.Color.Red);

            graphicsLib.DrawText(indent, y += spacing, "Purple", Meadow.Foundation.Color.Purple);

            graphicsLib.DrawText(indent, y += spacing, "BlueViolet", Meadow.Foundation.Color.BlueViolet);

            graphicsLib.DrawText(indent, y += spacing, "Blue", Meadow.Foundation.Color.Blue);

            graphicsLib.DrawText(indent, y += spacing, "Cyan", Meadow.Foundation.Color.Cyan);

            graphicsLib.DrawText(indent, y += spacing, "LawnGreen", Meadow.Foundation.Color.LawnGreen);

            graphicsLib.DrawText(indent, y += spacing, "GreenYellow", Meadow.Foundation.Color.GreenYellow);

            graphicsLib.DrawText(indent, y += spacing, "Yellow", Meadow.Foundation.Color.Yellow);

            graphicsLib.DrawText(indent, y += spacing, "Orange", Meadow.Foundation.Color.Orange);

            graphicsLib.DrawText(indent, y += spacing, "Brown", Meadow.Foundation.Color.Brown);


            Console.WriteLine("Show");

            graphicsLib.Show();
            
            Console.WriteLine("Show complete");
        }
    }
}