using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace SSD1306Display_Sample
{
    public class SSD1306DisplayApp : App<F7Micro, SSD1306DisplayApp>
    {
        GraphicsLibrary graphics;
        SSD1306 display;

        public SSD1306DisplayApp()
        {
            //CreateSpiDisplay();
            CreateI2CDisplay();

            Console.WriteLine("Test display API");
            TestRawDisplayAPI();
            Thread.Sleep(1000);

            Console.WriteLine("Create Graphics Library");

            graphics = new GraphicsLibrary(display);

            graphics.Clear();
            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(0, 0, "Meadow F7");
            graphics.DrawRectangle(5, 14, 30, 10, true);

            graphics.Show();
        }

        void CreateSpiDisplay ()
        {
            Console.WriteLine("Create SpiBus");
            var spiBus = Device.CreateSpiBus();

            Console.WriteLine("Create Display");

            display = new SSD1306(device: Device, spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                SSD1306.DisplayType.OLED128x64);
        }

        void CreateI2CDisplay ()
        {
            Console.WriteLine("Create I2CBus");
            var i2CBus = Device.CreateI2cBus();

            Console.WriteLine("Create Display");
            display = new SSD1306(i2CBus, 60, SSD1306.DisplayType.OLED128x32);
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
    }
}
