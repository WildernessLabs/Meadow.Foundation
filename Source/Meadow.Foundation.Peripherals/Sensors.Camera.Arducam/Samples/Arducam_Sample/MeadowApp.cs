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

        Arducam camera;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            //camera = new Vc0706(Device, Device.PlatformOS.GetSerialPortName("COM4"), 38400);

            return Task.CompletedTask;
        }

        public async override Task Run()
        {
            await TakePicture();
        }

        async Task TakePicture()
        {
            camera.CapturePhoto();

            using var jpegStream = await camera.GetPhotoStream();

            var jpeg = new JpegImage(jpegStream);
            Resolver.Log.Info($"Image decoded - width:{jpeg.Width}, height:{jpeg.Height}");
        }

        //<!=SNOP=>
    }
}