using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace TftSpiDisplay_Sample
{
    public class ST7565DisplayApp : App<F7Micro, ST7565DisplayApp>
    {
        ST7565 display;
        ISpiBus spiBus;

        public ST7565DisplayApp()
        {
            Console.WriteLine("ST7565 sample");
            Console.WriteLine("Create Spi bus");

            spiBus = Device.CreateSpiBus();// Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, 2000);


            Console.WriteLine("Create display driver instance");
            display = new ST7565(device: Device, spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D00,
                resetPin: Device.Pins.D01,
                width: 128, height: 64);

            display.SetContrast(10);

            Console.WriteLine("Create graphics lib");

            var graphicsLib = new GraphicsLibrary(display);
            graphicsLib.CurrentFont = new Font8x8();

            graphicsLib.Clear();

            graphicsLib.DrawTriangle(10, 10, 50, 50, 10, 50, Meadow.Foundation.Color.Red);

            graphicsLib.DrawRectangle(20, 15, 40, 20, Meadow.Foundation.Color.Yellow, true);

            //graphicsLib.DrawCircle()

            graphicsLib.DrawText(5, 5, "Meadow F7 SPI");

            graphicsLib.Show();


          /*  Console.WriteLine("Clear display");
            display.Clear(true);
            
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
