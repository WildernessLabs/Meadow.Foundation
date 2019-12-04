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

            camera = new ArducamMini(Device.CreateI2cBus());

            Thread.Sleep(1000);

            Console.WriteLine("Attempting single capture");
            camera.FlushFifo();
            camera.ClearFifoFlag();
            camera.StartCapture();

            Console.WriteLine("Capture started");

            Thread.Sleep(1000);
        }
    }
}