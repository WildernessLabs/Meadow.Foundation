using BitMiracle.LibJpeg;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Camera;
using System.Threading.Tasks;

namespace Sensors.Camera.Arducam_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Arducam camera;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            camera = new Arducam(Device.CreateSpiBus(), Device.Pins.D00, Device.CreateI2cBus(), 0x60);

            return Task.CompletedTask;
        }

        public async override Task Run()
        {


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