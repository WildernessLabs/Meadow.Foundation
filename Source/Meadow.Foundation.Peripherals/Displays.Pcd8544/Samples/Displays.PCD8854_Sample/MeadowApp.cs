using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;

namespace Displays.PCD8854_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        PCD8544 pCD8544;
        GraphicsLibrary graphics;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            pCD8544 = new PCD8544
            (
                device: Device,
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D01,
                dcPin: Device.Pins.D00,
                resetPin: Device.Pins.D02
            );

            graphics = new GraphicsLibrary(pCD8544);

            TestPCD8544();
        }
        
        void TestPCD8544() 
        {
            Console.WriteLine("TestPCD8544...");

            // Drawing natively in the display
            pCD8544.Clear();
            for (int i = 0; i < 30; i++)
            {
                pCD8544.DrawPixel(i, i, true);
                pCD8544.DrawPixel(30 + i, i, true);
                pCD8544.DrawPixel(60 + i, i, true);
            }
            pCD8544.Show();

            // Drawing with Display Graphics Library
            //graphics.Clear(true);
            //graphics.CurrentFont = new Font8x12();
            //graphics.DrawText(0, 0, "Meadow F7");
            //graphics.DrawRectangle(5, 14, 30, 10, true);
        }
    }
}