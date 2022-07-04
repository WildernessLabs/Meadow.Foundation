using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Ssd130x;
using Meadow.Foundation.Graphics;
using Meadow.Units;

namespace Displays.Ssd130x.Ssd1309_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        MicroGraphics graphics;
        Ssd1309 display;

        public MeadowApp()
        {
            CreateSpiDisplay();
            //CreateI2CDisplay();

            Console.WriteLine("Create canvas...");
            graphics = new MicroGraphics(display);

            graphics.Clear();
            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(0, 0, "Meadow F7", Meadow.Foundation.Color.White);
            graphics.DrawRectangle(5, 14, 30, 10, true);

            Console.WriteLine("Show...");
            graphics.Show();
            Console.WriteLine("Show Complete");
        }

        void CreateSpiDisplay()
        {
            Console.WriteLine("Create Display with SPI...");

            var config = new Meadow.Hardware.SpiClockConfiguration(new Frequency(6000, Frequency.UnitType.Kilohertz), Meadow.Hardware.SpiClockConfiguration.Mode.Mode0);

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

        //<!=SNOP=>

        void TestCircles()
        {
            graphics.Clear();
            // graphics.DrawCircle(20, 20, 1); 
            graphics.DrawCircle(20, 20, 2); 
            graphics.DrawCircle(20, 20, 5); 
            graphics.DrawCircle(20, 20, 8); 
            graphics.DrawCircle(20, 20, 12); 
            graphics.Show();
        }

        void Grid()
        {
            int xOffset = 0;
            int yOffset = 0;
            int spacing = 15;

            for (int t = 0; t < 2000; t++)
            {
                display.Clear();

                for (int i = xOffset; i < display.Width; i += spacing)
                {
                    graphics.DrawVerticalLine(i, 0, (int)display.Height, true);
                }

                for (int j = yOffset; j < display.Height; j += spacing)
                {
                    graphics.DrawHorizontalLine(0, j, (int)display.Width, true);
                }

                xOffset = (xOffset + 1) % spacing;
                yOffset = (yOffset + 1) % spacing;

                display.Show();
            }
        }

        void Count()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();

            graphics = new MicroGraphics(display);
            graphics.CurrentFont = new Font8x12();

            stopwatch.Start();

            for (int i = 0; i < 9999; i++)
            {
                display.Clear();
                graphics.DrawText(0, 0, $"{i}");

                display.Show();
            }

            stopwatch.Stop();

            Console.WriteLine($"FPS: {10000.0 / stopwatch.Elapsed.TotalSeconds}");

            Thread.Sleep(100);
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
                display.DrawPixel(x, y, true);

                display.Show();

                x += xV;
                y += yV;

                if (x <= 0 || x >= display.Width - 1)
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
                display.DrawPixel(i, i, true);
                display.DrawPixel(30 + i, i, true);
                display.DrawPixel(60 + i, i, true);
            }

            Console.WriteLine("Show");
            display.Show();
        }

        void TestDisplayGraphicsAPI()
        {
            graphics = new MicroGraphics(display);

            graphics.Clear();
            graphics.CurrentFont = new Font12x16();
            graphics.DrawText(0, 0, "MeadowB3.7");
            graphics.DrawText(0, 24, "4-8x faster");
            graphics.DrawText(0, 48, "86x IO perf");

            graphics.Show();
        }
    }
}