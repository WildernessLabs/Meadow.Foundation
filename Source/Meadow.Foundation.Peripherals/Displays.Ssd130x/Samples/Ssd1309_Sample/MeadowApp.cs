using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Displays.Ssd130x.Ssd1309_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        MicroGraphics graphics;
        Ssd1309 display;

        public override Task Initialize(string[]? args)
        {
            CreateSpiDisplay();
            //CreateI2CDisplay();

            Resolver.Log.Info("Create canvas...");
            graphics = new MicroGraphics(display);

            return base.Initialize(args);
        }

        void CreateSpiDisplay()
        {
            Resolver.Log.Info("Create Display with SPI...");

            var config = new Meadow.Hardware.SpiClockConfiguration(new Frequency(6000, Frequency.UnitType.Kilohertz), Meadow.Hardware.SpiClockConfiguration.Mode.Mode0);

            var bus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            display = new Ssd1309
            (
                spiBus: bus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00
            );
        }

        void CreateI2CDisplay()
        {
            Resolver.Log.Info("Create Display with I2C...");

            display = new Ssd1309
            (
                i2cBus: Device.CreateI2cBus(),
                address: 60
            );
        }

        public override Task Run()
        {
            graphics.Clear();
            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(0, 0, "Meadow F7", Meadow.Foundation.Color.White);
            graphics.DrawRectangle(5, 14, 30, 10, true);

            Resolver.Log.Info("Show...");
            graphics.Show();
            Resolver.Log.Info("Show Complete");

            return base.Run();
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

            Resolver.Log.Info($"FPS: {10000.0 / stopwatch.Elapsed.TotalSeconds}");

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
            Resolver.Log.Info("Clear display");
            display.Clear(true);

            for (int i = 0; i < 30; i++)
            {
                display.DrawPixel(i, i, true);
                display.DrawPixel(30 + i, i, true);
                display.DrawPixel(60 + i, i, true);
            }

            Resolver.Log.Info("Show");
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