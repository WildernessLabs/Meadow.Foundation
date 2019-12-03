using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;

namespace Displays.PCD8854_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        PCD8544 display;
        GraphicsLibrary graphics;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            var config = new Meadow.Hardware.SpiClockConfiguration(PCD8544.DEFAULT_SPEED, Meadow.Hardware.SpiClockConfiguration.Mode.Mode0);

            display = new PCD8544
            (
                device: Device,
                spiBus: Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config),
                chipSelectPin: Device.Pins.D01,
                dcPin: Device.Pins.D00,
                resetPin: Device.Pins.D02
            );

            graphics = new GraphicsLibrary(display);
            graphics.Rotation = GraphicsLibrary.RotationType._180Degrees;

            TestPCD8544();

            Thread.Sleep(10000);

            CounterDemo();
        }

        void CounterDemo()
        {
            int count = 0;

            graphics.CurrentFont = new Font12x20();

            while(true)
            {
                graphics.Clear();
                graphics.DrawText(0, 0, $"Count:");
                graphics.DrawText(0, 24, $"{count}");
                graphics.Show();
                count++;
            }
        }
        
        void TestPCD8544() 
        {
            Console.WriteLine("TestPCD8544...");

            // Drawing with Display Graphics Library
            graphics.Clear(true);
            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(0, 0, "PCD8544");
            graphics.DrawRectangle(5, 14, 30, 10, true);

            graphics.Show();
        }
    }
}