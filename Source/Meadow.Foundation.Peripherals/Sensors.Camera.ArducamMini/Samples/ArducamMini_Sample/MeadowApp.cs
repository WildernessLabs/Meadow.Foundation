using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Camera;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        ArducamMini camera;

        public override async Task Initialize()
        {
            Console.WriteLine("Initialize...");

            var spiBus = Device.CreateSpiBus(new Meadow.Units.Frequency(8, Meadow.Units.Frequency.UnitType.Megahertz));
            camera = new ArducamMini(Device, spiBus, Device.Pins.D00, Device.CreateI2cBus());

            await Task.Delay(1000);
        }

        public override Task Run()
        {
            Console.WriteLine("Attempting single capture");
            camera.FlushFifo();
            camera.FlushFifo();
            camera.CapturePhoto();

            Console.WriteLine("Capture started");

            Thread.Sleep(1000);

            if (camera.IsPhotoAvaliable())
            {
                Console.WriteLine("Capture complete");

                var data = camera.GetImageData();

                Console.WriteLine($"Jpeg captured {data.Length}");
            }

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}