using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;

namespace Displays.Ssd1309_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        GraphicsLibrary graphics;
        Ssd1309 display;

        public MeadowApp()
        {
            //CreateSpiDisplay();
            CreateI2CDisplay();

            Console.WriteLine("Test display API");
            TestRawDisplayAPI();
            Thread.Sleep(1000);

            Console.WriteLine("Create Graphics Library");
            TestDisplayGraphicsAPI();
            Thread.Sleep(Timeout.Infinite);
        }

        //untested
        void CreateSpiDisplay()
        {
            Console.WriteLine("Create Display with SPI...");

            display = new Ssd1309
            (
                device: Device, 
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00
            );
        }

        void CreateI2CDisplay()
        {
            Console.WriteLine("Create Display with I2C...");

            display = new Ssd1309
            (
                i2cBus: Device.CreateI2cBus(), 
                address: 60
            );
        }

        void TestRawDisplayAPI()
        {
            display.Clear(true);

            for (int i = 0; i < 30; i++)
            {
                display.DrawPixel(i, i, true);
                display.DrawPixel(30 + i, i, true);
                display.DrawPixel(60 + i, i, true);
            }

            display.Show();
        }

        void TestDisplayGraphicsAPI() 
        {
            graphics = new GraphicsLibrary(display);

            graphics.Clear();
            graphics.CurrentFont = new Font12x16();
            graphics.DrawText(0, 0, "SSD1309");
            graphics.DrawText(0, 24, "MeadowB3.7");
            graphics.DrawText(0, 48, "4-8x faster");

            graphics.Show();
        }
    }
}