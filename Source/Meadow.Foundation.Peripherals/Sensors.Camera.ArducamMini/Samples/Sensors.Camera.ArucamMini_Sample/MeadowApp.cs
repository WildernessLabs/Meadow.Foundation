using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Camera;
using Meadow.Hardware;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        ArducamMini camera;

        public MeadowApp()
        {
            Initialize();
        }

        public void Initialize()
        {
            Console.WriteLine("Creating output ports...");

            camera = new ArducamMini(Device, Device.CreateSpiBus(), Device.Pins.D00, Device.CreateI2cBus());

            Thread.Sleep(1000);

            Console.WriteLine("Attempting single capture");
            camera.FlushFifo();
            camera.ClearFifoFlag();
            camera.StartCapture();

            Console.WriteLine("Capture started");

            Thread.Sleep(1000);

            if(camera.IsCaptureComplete())
            {
                Console.WriteLine("Capture complete");

                var data = camera.GetImageData();

                Console.WriteLine($"Jpeg captured {data.Length}");
            }
        }
    }
}