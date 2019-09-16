using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.ePaper;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace WaveShare_ePaper_Sample
{
    public class WaveShareEPaperApp : App<F7Micro, WaveShareEPaperApp>
    {
        IDigitalOutputPort redLed;
        IDigitalOutputPort blueLed;
        IDigitalOutputPort greenLed;

        EPD2i9b display;
        ISpiBus spiBus;

        public WaveShareEPaperApp()
        {
            Console.WriteLine("TftSpi sample");
            Console.WriteLine("Create Spi bus");

            spiBus = Device.CreateSpiBus();// Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, 2000);


            Console.WriteLine("Create display driver instance");
            display = new EPD2i9b(device: Device, spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                busyPin: Device.Pins.D03);

            display.Clear();

            for(int i = 0; i < 20; i++)
            {
                display.DrawPixel(i, i, true);
                display.DrawPixel(i, i + 2, false);
            }

            display.Show();

            Console.WriteLine("Create graphics lib");

            var graphics = new GraphicsLibrary(display);

            graphics.Clear();

            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(0, 0, "ePaper on Meadow");

            graphics.Show();




            //  ConfigurePorts();
            //  BlinkLeds();
        }

        public void ConfigurePorts()
        {
            Console.WriteLine("Creating Outputs...");
            redLed = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedRed);
            blueLed = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedBlue);
            greenLed = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedGreen);
        }

        public void BlinkLeds()
        {
            var state = false;

            while (true)
            {
                state = !state;

                Console.WriteLine($"State: {state}");

                redLed.State = state;
                Thread.Sleep(500);
                blueLed.State = state;
                Thread.Sleep(500);
                greenLed.State = state;
                Thread.Sleep(500);
            }
        }
    }
}
