using System;
using System.Threading.Tasks;
using BitMiracle.LibJpeg;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Camera;

namespace Sensors.Camera.Vc0706_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Vc0706 camera;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            camera = new Vc0706(Device, Device.SerialPortNames.Com4, 38400);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            if (!camera.SetCaptureResolution(Vc0706.ImageResolution._160x120))
            {
                Console.WriteLine("Set resolution failed");
            }

            _ = TakePicture();

            return Task.CompletedTask;
        }

        async Task TakePicture()
        {
            Console.WriteLine($"Image size is {camera.GetCaptureResolution()}");

            camera.CapturePhoto();

            using var jpegStream = await camera.GetPhotoStream();

            var jpeg = new JpegImage(jpegStream);
            Console.WriteLine($"Image decoded - width:{jpeg.Width}, height:{jpeg.Height}");
        }

        //<!=SNOP=>
    }
}