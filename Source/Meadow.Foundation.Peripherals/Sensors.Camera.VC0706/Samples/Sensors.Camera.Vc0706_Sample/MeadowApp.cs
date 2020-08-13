using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Camera;

namespace Sensors.Camera.Vc0706_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Vc0706 camera;
        RgbPwmLed onboardLed;

        public MeadowApp()
        {
            Initialize();

            CameraTest();
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

            Console.WriteLine("Init camera");

            camera = new Vc0706(Device, Device.SerialPortNames.Com4, 57600);

            Console.WriteLine("Init complete");
        }

        void CameraTest()
        {
            Console.WriteLine("Set image size");
            camera.SetImageSize(Vc0706.ImageSize.VC0706_160x120);

            Console.WriteLine($"Image size is {camera.GetImageSize()}");

            Console.WriteLine("Set TV off");
            camera.TvOff();
            Thread.Sleep(2000);
            Console.WriteLine("Set TV on");
            camera.TvOn();

            Console.WriteLine("Test complete");
        }
    }
}