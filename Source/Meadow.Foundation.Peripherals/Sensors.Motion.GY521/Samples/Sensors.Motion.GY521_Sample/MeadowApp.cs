using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace Sensors.Motion.GY521_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        GY521 gY521;

        public MeadowApp()
        {
            Console.WriteLine("Initializing");

            gY521 = new GY521(Device.CreateI2cBus());

            TestGY521();
        }

        private void TestGY521()
        {
            Console.WriteLine("TestGY521...");

            // Wake
            gY521.Wake();

            while (true)
            {
                Console.WriteLine("Reading...");
                gY521.Refresh();

                Console.WriteLine($" ({gY521.AccelerationX:X4},{gY521.AccelerationY:X4},{gY521.AccelerationZ:X4}) ({gY521.GyroX:X4},{gY521.GyroY:X4},{gY521.GyroZ:X4}) {gY521.Temperature}");

                Thread.Sleep(2000);
            }
        }
    }
}