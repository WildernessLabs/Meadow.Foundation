using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Tft;
using Meadow.Foundation.Graphics;

namespace Displays.Tft.ILI9163_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        protected ILI9163 iLI9163;
        protected GraphicsLibrary graphics;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            iLI9163 = new ILI9163
            (
                device: Device, 
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 128, height: 160
            );

            graphics = new GraphicsLibrary(iLI9163);

            TestILI9163();
        }

        protected void TestILI9163() 
        {
            Console.WriteLine("TestILI9163...");

            // Drawing natively in the display
            iLI9163.ClearScreen(250);
            iLI9163.Refresh();
            for (int i = 0; i < 30; i++)
            {
                iLI9163.DrawPixel(i, i, true);
                iLI9163.DrawPixel(30 + i, i, true);
                iLI9163.DrawPixel(60 + i, i, true);
            }
            iLI9163.Show();

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