using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Hardware;

namespace Leds.APA102_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Apa102 apa102;

        public MeadowApp()
        {
            Initialize();
            Run();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            ISpiBus spiBus = Device.CreateSpiBus();

            //Not used but is need to create the SPI Peripheral
            IDigitalOutputPort spiPeriphChipSelect = Device.CreateDigitalOutputPort(Device.Pins.D04);
            apa102 = new Apa102(spiBus, spiPeriphChipSelect, 10, Apa102.PixelOrder.BGR);
        }

        void Run()
        {
            Console.WriteLine("Run...");
            apa102.Clear();
            apa102.Show();

            Thread.Sleep(2000);
            apa102.SetLed(0, Color.Red, 0.5f);
            apa102.SetLed(1, Color.White);
            apa102.SetLed(2, Color.Blue);

            Thread.Sleep(2000);
            apa102.Show();

            Thread.Sleep(2000);
            apa102.AutoWrite = true;
            apa102.SetLed(0, Color.Green);
            apa102.SetLed(1, Color.Yellow);
            apa102.SetLed(2, Color.Pink);

            Thread.Sleep(5000);
            apa102.Clear();

        }
    }
}