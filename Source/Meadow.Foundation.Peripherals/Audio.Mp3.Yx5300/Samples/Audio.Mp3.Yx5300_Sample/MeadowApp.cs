using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Audio.Mp3;
using Meadow.Foundation.Leds;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Yx5300 mp3Player;

        RgbPwmLed onboardLed;

        public MeadowApp()
        {
            Initialize();

            onboardLed.SetColor(Color.Yellow);

            Mp3Test();
        }

        async Task Mp3Test()
        {
            onboardLed.SetColor(Color.Green);

            mp3Player.SetVolume(15);

            var status = await mp3Player.GetStatus();
            Console.WriteLine($"Status: {status}");

            var count = await mp3Player.GetNumberOfTracksInFolder(0);
            Console.WriteLine($"Count: {count}");

            mp3Player.Play();
            await Task.Delay(5000);

            status = await mp3Player.GetStatus();
            Console.WriteLine($"Status: {status}");

            mp3Player.Next();

            onboardLed.SetColor(Color.Blue);
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            onboardLed = new RgbPwmLed(device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                3.3f, 3.3f, 3.3f,
                Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode);

            mp3Player = new Yx5300(Device, Device.SerialPortNames.Com4);
        }
    }
}