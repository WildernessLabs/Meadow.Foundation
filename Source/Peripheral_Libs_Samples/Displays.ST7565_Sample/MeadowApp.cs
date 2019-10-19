using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;

namespace Displays.ST7565_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        ST7565 sT7565;
        GraphicsLibrary graphics;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            sT7565 = new ST7565
            (
                device: Device, 
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D00,
                resetPin: Device.Pins.D01,
                width: 128, 
                height: 64
            );

            graphics = new GraphicsLibrary(sT7565);

            TestST7565();
        }

        void TestST7565()
        {
            Console.WriteLine("TestST7565...");

            // Drawing natively in the display
            sT7565.Clear(true);
            for (int i = 0; i < 30; i++)
            {
                sT7565.DrawPixel(i, i, true);
                sT7565.DrawPixel(30 + i, i, true);
                sT7565.DrawPixel(60 + i, i, true);
            }
            sT7565.Show();

            // Drawing with Display Graphics Library
            //graphics.CurrentFont = new Font8x8();
            //graphics.Clear();
            //graphics.DrawTriangle(10, 10, 50, 50, 10, 50, Meadow.Foundation.Color.Red);
            //graphics.DrawRectangle(20, 15, 40, 20, Meadow.Foundation.Color.Yellow, true);            
            //graphics.DrawText(5, 5, "Meadow F7 SPI");
            //graphics.Show();
        }
    }
}