using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Transceivers;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Transceivers.Nrf24l01_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        RgbLed led;
        Nrf24l01 Radio;
        byte[] address = new byte[6] { 0, 0, 0, 0, 0, 1 };

        public MeadowApp()
        {
            led = new RgbLed(Device, Device.Pins.OnboardLedRed, Device.Pins.OnboardLedGreen, Device.Pins.OnboardLedBlue);
            led.SetColor(RgbLed.Colors.Red);

            var config = new SpiClockConfiguration(10000000, SpiClockConfiguration.Mode.Mode0);
            ISpiBus spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            Radio = new Nrf24l01(
                    device: Device,
                    spiBus: spiBus,
                    chipEnablePin: Device.Pins.D11,
                    chipSelectLine: Device.Pins.D10,
                    interruptPin: Device.Pins.D09);
            Radio.OpenReadingPipe(0, address);
            Radio.SetPALevel(0);
            Radio.StartListening();

            led.SetColor(RgbLed.Colors.Green);

            while (true)
            {
                if (Radio.available())
                {
                    Console.WriteLine("Hey");
                }

                Thread.Sleep(500);
            }            
        }

    }
}
