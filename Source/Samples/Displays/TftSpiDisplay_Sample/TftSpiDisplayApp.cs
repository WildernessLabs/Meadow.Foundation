using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Tft;
using Meadow.Hardware;

namespace TftSpiDisplay_Sample
{
    public class TftSpiDisplayApp : App<F7Micro, TftSpiDisplayApp>
    {
        SSD1351 display;
        ISpiBus spiBus;

        public TftSpiDisplayApp()
        {
            Console.WriteLine("TftSpi sample");
            Console.WriteLine("Create Spi bus");

            spiBus = Device.CreateSpiBus();

            Console.WriteLine("Create SSD1351 driver instance");
            display = new SSD1351(device: Device, spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 128, height: 128,
                speedKHz: 9500);

            Console.WriteLine("Clear display");
            display.Clear(true);

            Console.WriteLine("Draw lines");
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
