using System;
using System.Threading.Tasks;
using BitMiracle.LibJpeg;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Camera;

namespace Sensors.Camera.Vc0706_Sample
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        //<!=SNIP=>

        readonly Vc0706 camera;

        //Uses SimpleJpegDecoder package for jpeg decoding
        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");

            camera = new Vc0706(Device, Device.SerialPortNames.Com4, 38400);

            Console.WriteLine("Set resolution");

            if (!camera.SetCaptureResolution(Vc0706.ImageResolution._160x120))
            {
                Console.WriteLine("Set resolution failed");
            }

            _ = TakePicture();
        }

        async Task TakePicture()
        {
            Console.WriteLine($"Image size is {camera.GetCaptureResolution()}");

            camera.TakePicture();

            using var jpegStream = await camera.GetImageStream();

            var jpeg = new JpegImage(jpegStream);
            Console.WriteLine($"{jpeg.Width}, {jpeg.Height}");
        }

        //<!=SNOP=>
    }
}