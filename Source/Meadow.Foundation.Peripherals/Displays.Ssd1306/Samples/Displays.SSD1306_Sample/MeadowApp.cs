using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;

namespace Displays.SSD1306_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        GraphicsLibrary graphics;
        SSD1306 display;

        public MeadowApp()
        {
            //CreateSpiDisplay();
            CreateI2CDisplay();

            Console.WriteLine("Test display API");
            TestRawDisplayAPI();
            Thread.Sleep(1000);

            Console.WriteLine("Create Graphics Library");
            TestDisplayGraphicsAPI();
            Thread.Sleep(Timeout.Infinite);
        }

        void CreateSpiDisplay()
        {
            Console.WriteLine("Create Display with SPI...");

            display = new SSD1306
            (
                device: Device, 
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                displayType: SSD1306.DisplayType.OLED128x64
            );
        }

        void CreateI2CDisplay()
        {
            Console.WriteLine("Create Display with I2C...");

            display = new SSD1306
            (
                i2cBus: Device.CreateI2cBus(), 
                address: 60, 
                displayType: SSD1306.DisplayType.OLED128x32
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
            graphics.DrawText(0, 0, "Meadow F7");
            graphics.DrawRectangle(5, 14, 30, 10, true);

            graphics.Show();
        }
    }
}