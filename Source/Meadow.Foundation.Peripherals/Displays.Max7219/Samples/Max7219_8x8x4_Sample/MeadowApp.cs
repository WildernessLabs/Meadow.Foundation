using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Displays;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        MicroGraphics graphics;
        Max7219 display;

        public MeadowApp()
        {
            Init();

            while (true)
            {
                ShowText();
                Thread.Sleep(2000);

                ScrollText();
                Thread.Sleep(2000);

                Counter();
                Thread.Sleep(2000);

                DrawPixels();
                Thread.Sleep(2000);
            }
        }

        void Init()
        {
            Console.WriteLine("Init...");

            display = new Max7219(
                Device, Device.CreateSpiBus(Max7219.DefaultSpiBusSpeed),
                Device.Pins.D00, deviceCount: 4,
                maxMode: Max7219.Max7219Mode.Display);

            graphics = new MicroGraphics(display)
            {
                CurrentFont = new Font4x8(),
                IgnoreOutOfBoundsPixels = true,
            };

            Console.WriteLine($"Display W: {display.Width}, H: {display.Height}");

            graphics.Rotation = RotationType._90Degrees;

            Console.WriteLine($"Graphics W: {graphics.Width}, H: {graphics.Height}");

            Console.WriteLine("Max7219 instantiated");

            graphics.Clear();
            graphics.DrawRectangle(0, 0, graphics.Width, graphics.Height);

            graphics.Show();

            Thread.Sleep(2000);
        }

        void ScrollText()
        {
            graphics.CurrentFont = new Font6x8();

            string message = "Meadow F7 by Wilderness Labs";

            int delta = graphics.MeasureText(message).Width - graphics.Width;

            for(int i = 0; i < delta; i++)
            {
                graphics.Clear();
                graphics.DrawText(0 - i, 0, message);
                graphics.Show();
                Thread.Sleep(50);
            }
        }

        void DrawPixels()
        {
            Console.WriteLine("Clear");
            display.Clear();
            Console.WriteLine("Draw");
            for (int i = 0; i < 8; i++)
            {
                display.DrawPixel(i, i, true);
            }
            display.Show();
        }

        void ShowText()
        {
            graphics.CurrentFont = new Font4x8();

            //Graphics Lib
            Console.WriteLine("Clear");
            graphics.Clear();
            graphics.DrawText(0, 1, "MEADOWF7");
            Console.WriteLine("Show");
            graphics.Show();
        }

        void Counter()
        {
            graphics.CurrentFont = new Font8x8();

            for (int i = 0; i < 1000; i++)
            {
                graphics.Clear();
                graphics.DrawText(0, 0, $"{i}");
                graphics.Show();
            }
        }
    }
}