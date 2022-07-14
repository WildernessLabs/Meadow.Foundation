using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Transceivers;
using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using Meadow.Units;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Transceivers.Nrf24l01_RX_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        RgbLed led;
        Nrf24l01 radio;
        string address = "00001";

        Memory<byte> readBuffer;

        public override Task Initialize()
        {
            led = new RgbLed(Device, Device.Pins.OnboardLedRed, Device.Pins.OnboardLedGreen, Device.Pins.OnboardLedBlue);
            led.SetColor(RgbLedColors.Red);

            var config = new SpiClockConfiguration(new Frequency(12000, Frequency.UnitType.Kilohertz), SpiClockConfiguration.Mode.Mode0);
            ISpiBus spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            radio = new Nrf24l01(
                device: Device,
                spiBus: spiBus,
                chipEnablePin: Device.Pins.D13,
                chipSelectLine: Device.Pins.D12,
                interruptPin: Device.Pins.D00);

            return Task.CompletedTask;
        }

        public async override Task Run()
        { 
            radio.SetChannel(76);
            radio.OpenReadingPipe(0, Encoding.UTF8.GetBytes(address));
            radio.SetPALevel(0);
            radio.StartListening();

            led.SetColor(RgbLedColors.Green);

            while (true)
            {
                if (radio.IsAvailable())
                {
                    readBuffer = radio.Read(32);

                    await Task.Delay(1000);
                }
            }           
        }
    }
}