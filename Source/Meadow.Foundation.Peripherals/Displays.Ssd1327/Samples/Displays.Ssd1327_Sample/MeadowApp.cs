using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using System;
using System.Threading;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        RgbPwmLed onboardLed;

        Ssd1327 display;
        GraphicsLibrary graphics;

        public MeadowApp()
        {
            Initialize();

            Console.WriteLine("Init complete");

            onboardLed.SetColor(Color.Green);

            display.Clear();
            for (int i = 0; i < 32; i++)
            {
                display.DrawPixel(i, i, true);
            }
            Console.WriteLine("Simple line");
            display.Show();

            Thread.Sleep(2000);

            graphics.Clear();

            for(int i = 10; i > 0; i--)
            {
                graphics.DrawText(0, i * 12, "SSD1327", Color.FromRgb(i * 0.1, i * 0.1, i * 0.1));
            }

            Console.WriteLine("Show Text");
            graphics.Show();

            Thread.Sleep(5000);

            TestDisplay();

            Console.WriteLine("Finished");

        }

        void TestDisplay()
        { 
            while (true)
            {
                PolarLineTest();
                Thread.Sleep(5000);

                RoundRectTest();
                Thread.Sleep(5000);

                QuadrantTest();
                Thread.Sleep(5000);

                ColorFontTest();
                Thread.Sleep(5000);
            }

            onboardLed.SetColor(Color.Red);
        }

        void Initialize()
        {
            Console.WriteLine("Initialize display...");

            var spiBus = Device.CreateSpiBus();

            display = new Ssd1327(Device, spiBus, Device.Pins.D02, Device.Pins.D01, Device.Pins.D00);

            display.SetContrast(60);

            graphics = new GraphicsLibrary(display);
            graphics.CurrentFont = new Font8x12();

            Console.WriteLine("Initialize led...");

            onboardLed = new RgbPwmLed(device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                3.3f, 3.3f, 3.3f,
                Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode);
        }

        void PolarLineTest()
        {
            graphics.Clear();
            graphics.Stroke = 1;

            for (int i = 0; i < 270; i += 12)
            {
                graphics.DrawLine(64, 64, 60, (float)(i * Math.PI / 180), Color.White);
            }

            graphics.Show();
        }

        void RoundRectTest()
        {
            graphics.Clear();

            graphics.Stroke = 1;

            graphics.DrawRoundedRectangle(10, 10, 100, 100, 10, Color.Orange, false);

            graphics.DrawRoundedRectangle(20, 20, 100, 60, 20, Color.Blue, true);

            graphics.Show();
        }

        void QuadrantTest()
        {
            graphics.Clear();

            graphics.DrawCircleQuadrant(64, 64, 60, 0, Color.Yellow, true);
            graphics.DrawCircleQuadrant(64, 64, 60, 1, Color.Blue, true);
            graphics.DrawCircleQuadrant(64, 64, 60, 2, Color.Cyan, true);
            graphics.DrawCircleQuadrant(64, 64, 60, 3, Color.LawnGreen, true);

            graphics.Show();
        }

        void ColorFontTest()
        {
            graphics.CurrentFont = new Font8x12();

            graphics.Clear();

            graphics.DrawTriangle(120, 20, 200, 100, 120, 100, Meadow.Foundation.Color.Red, false);

            graphics.DrawRectangle(140, 30, 40, 90, Meadow.Foundation.Color.Yellow, false);

            graphics.DrawCircle(160, 80, 40, Meadow.Foundation.Color.Cyan, false);

            int indent = 5;
            int spacing = 14;
            int y = indent;

            graphics.DrawText(indent, y, "Meadow + SSD1327!!");

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

            Console.WriteLine("Show complete");
        }
    }
}