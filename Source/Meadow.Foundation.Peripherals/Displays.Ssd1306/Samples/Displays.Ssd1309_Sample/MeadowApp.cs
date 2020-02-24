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
            CreateSpiDisplay();
            //CreateI2CDisplay();

            display.Clear(true);

            Console.WriteLine("Test display API");
            TestRawDisplayAPI();
            Thread.Sleep(5000);

            Console.WriteLine("Create Graphics Library");
            TestDisplayGraphicsAPI();
            Thread.Sleep(10000);

            Bounce();
        }

        //untested
        void CreateSpiDisplay()
        {
            Console.WriteLine("Create Display with SPI...");

            var config = new Meadow.Hardware.SpiClockConfiguration(6000, Meadow.Hardware.SpiClockConfiguration.Mode.Mode0);

            var bus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            display = new Ssd1309
            (
                device: Device, 
                spiBus: bus, 
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

        void Bounce()
        {
            int x = 1;
            int y = 1;
            int xV = 1;
            int yV = 1;

            while (true)
            {
                display.Clear();
                display.DrawPixel(x, y);

                display.Show();

                x += xV;
                y += yV;

                if(x <= 0 || x >= display.Width - 1)
                {
                    xV *= -1;
                }

                if (y <= 0 || y >= display.Height - 1)
                {
                    yV *= -1;
                }
            }
        }

        void TestRawDisplayAPI()
        {
            Console.WriteLine("Clear display");
            display.Clear(true);

            for (int i = 0; i < 30; i++)
            {
                Console.WriteLine($"Draw pixel {i}, {i}");
                display.DrawPixel(i, i, true);
                display.DrawPixel(30 + i, i, true);
                display.DrawPixel(60 + i, i, true);
            }

            Console.WriteLine("Show");
            display.Show();
        }

        void TestDisplayGraphicsAPI() 
        {
            graphics = new GraphicsLibrary(display);

            graphics.Clear();
            graphics.CurrentFont = new Font12x16();
            graphics.DrawText(0, 0, "MeadowB3.7");
            graphics.DrawText(0, 24, "4-8x faster");
            graphics.DrawText(0, 48, "86x IO perf");

            graphics.Show();
        }
    }
}