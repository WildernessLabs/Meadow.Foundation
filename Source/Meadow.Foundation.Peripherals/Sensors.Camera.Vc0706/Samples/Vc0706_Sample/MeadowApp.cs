using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Camera;
using SimpleJpegDecoder;
using System;
using System.Threading.Tasks;

namespace Sensors.Camera.Vc0706_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Vc0706 camera;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            camera = new Vc0706(Device, Device.PlatformOS.GetSerialPortName("COM4"), 38400);

            return Task.CompletedTask;
        }

        public async override Task Run()
        {
            if (!camera.SetCaptureResolution(Vc0706.ImageResolution._160x120))
            {
                Resolver.Log.Info("Set resolution failed");
            }

            var jpegData = await camera.CapturePhoto();

            if (jpegData.Length > 0)
            {
                var decoder = new JpegDecoder();
                var jpg = decoder.DecodeJpeg(jpegData);
                Console.WriteLine($"Jpeg decoded is {jpg.Length} bytes, W: {decoder.Width}, H: {decoder.Height}");
            }
            else
            {
                Console.WriteLine("Image capture failed");
            }
        }

        //<!=SNOP=>
    }
}