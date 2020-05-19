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
        APA102Led _apa102Led;

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
            _apa102Led = new APA102Led(spiBus, spiPeriphChipSelect, 10, APA102Led.PixelOrder.BGR);
        }

        void Run()
        {
            Console.WriteLine("Run...");
            _apa102Led.Clear();
            _apa102Led.Show();

            Thread.Sleep(2000);
            _apa102Led.SetLed(0, Color.Red, 0.5f);
            _apa102Led.SetLed(1, Color.White);
            _apa102Led.SetLed(2, Color.Blue);

            Thread.Sleep(2000);
            _apa102Led.Show();

            Thread.Sleep(2000);
            _apa102Led.AutoWrite = true;
            _apa102Led.SetLed(0, Color.Green);
            _apa102Led.SetLed(1, Color.Yellow);
            _apa102Led.SetLed(2, Color.Pink);

            Thread.Sleep(5000);
            _apa102Led.Clear();

        }
    }
}
