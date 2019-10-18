using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Tft;
using Meadow.Foundation.Graphics;

namespace Displays.Tft.ST7789_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        protected ST7789 sT7789;
        protected GraphicsLibrary graphics;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            sT7789 = new ST7789
            (
                device: Device, 
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 240, height: 240
            );

            graphics = new GraphicsLibrary(sT7789);

            TestST7789();
        }

        protected void TestST7789()
        {
            Console.WriteLine("TestST7789...");

            // Drawing natively in the display
            sT7789.ClearScreen(250);
            sT7789.Refresh();
            for (int i = 0; i < 30; i++)
            {
                sT7789.DrawPixel(i, i, true);
                sT7789.DrawPixel(30 + i, i, true);
                sT7789.DrawPixel(60 + i, i, true);
            }
            sT7789.Show();

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