using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Tft;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace TftSpiDisplay_Sample
{
    public class TftSpiDisplayApp : App<F7Micro, TftSpiDisplayApp>
    {
        SSD1331 display;
        ISpiBus spiBus;

        public TftSpiDisplayApp()
        {
            Console.WriteLine("TftSpi sample");
            Console.WriteLine("Create Spi bus");

            spiBus = Device.CreateSpiBus();// Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, 2000);
            

            Console.WriteLine("Create SSD1351 driver instance");
            display = new SSD1331(device: Device, spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 96, height: 64);

            var graphicsLib = new GraphicsLibrary(display);
            graphicsLib.CurrentFont = new Font8x12();

            graphicsLib.Clear();

            graphicsLib.DrawTriangle(10, 10, 50, 50, 10, 50, Meadow.Foundation.Color.Red);

            graphicsLib.DrawRectangle(20, 15, 40, 20, Meadow.Foundation.Color.Yellow, true);

            graphicsLib.DrawText(5, 5, "Meadow F7 SPI");

            graphicsLib.Show();


            /*Console.WriteLine("Clear display");
            display.ClearScreen(250);
            display.Refresh();

            Console.WriteLine("Draw lines");
            for (int i = 0; i < 30; i++)
            {
                display.DrawPixel(i, i, true);
                display.DrawPixel(30 + i, i, true);
                display.DrawPixel(60 + i, i, true);
            }

            display.Show(); */
        }
    }
}
