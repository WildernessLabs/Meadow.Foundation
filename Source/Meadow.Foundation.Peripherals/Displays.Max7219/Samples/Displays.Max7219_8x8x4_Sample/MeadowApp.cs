using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Displays;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        GraphicsLibrary graphics;
        Max7219 display;

        public MeadowApp()
        {
            Init();

            while (true)
            {
                Counter();
                Thread.Sleep(2000);

                DrawPixels();
                Thread.Sleep(2000);

                ShowText();
                Thread.Sleep(2000);
            }
        }

        void Init()
        {
            Console.WriteLine("Init...");

            var spiBus = Device.CreateSpiBus(Max7219.SpiClockFrequency);

            display = new Max7219(Device, spiBus, Device.Pins.D01, 4, Max7219.Max7219Type.Display);

            graphics = new GraphicsLibrary(display);
            
            graphics.Rotation = GraphicsLibrary.RotationType._90Degrees;

            Console.WriteLine("Max7219 instantiated");
        }

        void DrawPixels()
        {
            Console.WriteLine("Clear");
            display.Clear();
            Console.WriteLine("Draw");
            for (int i = 0; i < 8; i++)
            {
                display.DrawPixel(i, i);
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
                Console.WriteLine("Show");
                graphics.Show();
            }
        }
    }
}