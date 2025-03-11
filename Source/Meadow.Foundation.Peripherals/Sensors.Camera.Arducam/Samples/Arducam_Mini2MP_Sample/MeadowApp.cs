using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Camera;
using SimpleJpegDecoder;
using System;
using System.Threading.Tasks;

namespace Sensors.Camera.Arducam_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        ArducamMini2MP camera;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            camera = new ArducamMini2MP(Device.CreateSpiBus(), Device.Pins.D00, Device.CreateI2cBus());

            Console.WriteLine("Camera initialized");

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Console.WriteLine("Run...");

            await camera.SetJpegSize(Arducam.ImageSize._160x120);

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