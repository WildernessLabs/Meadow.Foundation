using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;

namespace Displays.Ssd1306_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        GraphicsLibrary graphics;
        Ssd1306 display;

        public MeadowApp()
        {
            //CreateSpiDisplay();
            CreateI2CDisplay();

            Console.WriteLine("Fill display");
            for(int x = 0; x < display.Width; x++)
            {
                for(int y = 0; y < display.Height; y++)
                {
                    display.DrawPixel(x, y, true);

                }
            }
            display.Show();
            Thread.Sleep(2000);

         /*   Console.WriteLine("Test display API");
            TestRawDisplayAPI();
            Thread.Sleep(3000); */
           

            Console.WriteLine("Create Graphics Library");
            TestDisplayGraphicsAPI();
            Thread.Sleep(2000);

            Console.WriteLine("Test Inversion");
            for(int x = 0; x < 64; x++)
            {
                for(int y = 0; y < 12; y++)
                {
                    display.InvertPixel(x, y);
                }
            }
            display.Show();
            Thread.Sleep(3000);

            Console.WriteLine("Check offsets");

            graphics.Clear();
            graphics.DrawRectangle(0, 0, (int)display.Width, (int)display.Height, true, false);
            graphics.Show();

            Thread.Sleep(3000);

            for (int x = 0; x < display.Width; x++)
            {
               
                Console.WriteLine($"X: {x}");
                graphics.Clear();

                graphics.DrawLine(x, 0, x, (int)display.Height - 1, true);

                graphics.Show();

                Thread.Sleep(50);
            }

            for (int y = 0; y < display.Height; y++)
            {
                Console.WriteLine($"Y: {y}");
                graphics.Clear();

                graphics.DrawLine(0, y, (int)display.Width - 1, y, true);

                graphics.Show();

                Thread.Sleep(50);
            }

            Thread.Sleep(Timeout.Infinite);
        }

        void CreateSpiDisplay()
        {
            Console.WriteLine("Create Display with SPI...");

            display = new Ssd1306
            (
                device: Device, 
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                displayType: Ssd1306.DisplayType.OLED128x64
            );
        }

        void CreateI2CDisplay()
        {
            Console.WriteLine("Create Display with I2C...");

            display = new Ssd1306
            (
                i2cBus: Device.CreateI2cBus(), 
                address: 60, 
                displayType: Ssd1306.DisplayType.OLED128x32
            );
        }

        void TestRawDisplayAPI()
        {
            display.Clear(true);

            for (int i = 0; i < 30; i++)
            {
                display.DrawPixel(i, i, true);
                display.DrawPixel(30 + i, i, true);
                display.DrawPixel(60 + i, i, true);
            }

            display.Show();
        }

        void TestDisplayGraphicsAPI() 
        {
            graphics = new GraphicsLibrary(display);

            graphics.Clear();
            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(0, 0, "Meadow F7", Meadow.Foundation.Color.White);
            graphics.DrawRectangle(5, 14, 30, 10, true);

            graphics.Show();
        }
    }
}