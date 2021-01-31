using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.Tft;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace Displays.TftSpi.St7735_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        St7735 display;
        GraphicsLibrary graphics;

        public MeadowApp()
        {
            Initialize();

            ShapeTest();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            var spiBus = Device.CreateSpiBus();

            Console.WriteLine("Create display driver instance");

            
            /*display = new St7735(device: Device, spiBus: spiBus,
                chipSelectPin: Device.Pins.D10,
                dcPin: Device.Pins.D09,
                resetPin: Device.Pins.D05,
                width: 160, height: 80, St7735.DisplayType.ST7735R_80x160); */

            display = new St7735(device: Device, spiBus: spiBus,
              chipSelectPin: Device.Pins.D02,
              dcPin: Device.Pins.D01,
              resetPin: Device.Pins.D00,
              width: 128, height: 160, St7735.DisplayType.ST7735R);

            Console.WriteLine("Create graphics lib");

            graphics = new GraphicsLibrary(display);
        }

        void ShapeTest()
        {
            Console.WriteLine("Shape test");

            graphics.Clear();

            graphics.DrawCircle(60, 60, 20, Color.Purple);
            graphics.DrawRectangle(10, 10, 30, 60, Color.Red);
            graphics.DrawTriangle(20, 20, 10, 70, 60, 60, Color.Green);

            graphics.DrawCircle(90, 60, 20, Color.Cyan, true);
            graphics.DrawRectangle(100, 100, 30, 10, Color.Yellow, true);
            graphics.DrawTriangle(120, 20, 110, 70, 160, 60, Color.Pink, true);

            graphics.DrawLine(10, 120, 110, 130, Color.SlateGray);

            graphics.Show();

            Console.WriteLine("Shape test complete");
        }
    }
}