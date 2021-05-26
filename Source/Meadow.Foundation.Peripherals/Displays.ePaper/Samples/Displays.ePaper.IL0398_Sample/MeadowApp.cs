using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.ePaper;
using Meadow.Foundation.Graphics;

namespace Displays.ePaper.IL0398_Sample
{
    /* Driver in development */
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Il0398 display;

        public MeadowApp()
        {
            Console.WriteLine("ePaper sample");
            Console.WriteLine("Create Spi bus");

            var spiBus = Device.CreateSpiBus(48000);// Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, 2000);

            Console.WriteLine("Create display driver instance");
            display = new Il0398(device: Device, spiBus: spiBus,
                chipSelectPin: Device.Pins.D12,
                dcPin: Device.Pins.D13,
                resetPin: Device.Pins.D14,
                busyPin: Device.Pins.D15,
                width: 300,
                height: 400);

            var canvas = new GraphicsLibrary(display);

            //any color but black will show the ePaper alternate color 
            canvas.DrawRectangle(1, 1, 126, 32, Meadow.Foundation.Color.Red, false);

            canvas.CurrentFont = new Font8x12();
            canvas.DrawText(2, 2, "IL0398");
            canvas.DrawText(2, 20, "Meadow F7");

            int ySpacing = 6;

            for (int i = 0; i < 3; i++)
            {
                canvas.DrawLine(2, 70 + ySpacing * i, 22, 50 + ySpacing * i, true);
                canvas.DrawLine(22, 50 + ySpacing * i, 42, 70 + ySpacing * i, true);
                canvas.DrawLine(44, 70 + ySpacing * i, 64, 50 + ySpacing * i, true);
                canvas.DrawLine(64, 50 + ySpacing * i, 84, 70 + ySpacing * i, true);
                canvas.DrawLine(86, 70 + ySpacing * i, 106, 50 + ySpacing * i, true);
                canvas.DrawLine(106, 50 + ySpacing * i, 126, 70 + ySpacing * i, true);
            }

            Console.WriteLine("Show");

            canvas.Show();

            Console.WriteLine("Show Complete.");
        }
    }
}