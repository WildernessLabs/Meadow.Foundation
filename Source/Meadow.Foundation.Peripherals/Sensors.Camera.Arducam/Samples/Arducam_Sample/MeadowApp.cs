using BitMiracle.LibJpeg;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Camera;
using System.Threading;
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

            camera = new Arducam(Device.CreateSpiBus(), Device.Pins.D00, Device.CreateI2cBus(), (byte)Arducam.Addresses.Default);

            return Task.CompletedTask;
        }

        public async override Task Run()
        {
            Resolver.Log.Info("Run...");

            //Reset the CPLD
            camera.write_reg(0x07, 0x80);
            Thread.Sleep(100);
            camera.write_reg(0x07, 0x00);
            Thread.Sleep(100);

            Thread.Sleep(1000);
            camera.write_reg(0x00  /*ARDUCHIP_TEST1 */, 0x55);
            Thread.Sleep(5000);
            var temp = camera.read_reg(0x00);

            Resolver.Log.Info($"temp: {temp}");


            //     camera.clear_fifo_flag();

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