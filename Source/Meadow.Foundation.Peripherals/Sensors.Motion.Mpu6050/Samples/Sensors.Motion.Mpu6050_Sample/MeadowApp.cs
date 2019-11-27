using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace Sensors.Motion.mpu5060_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Mpu6050 mpu5060;

        public MeadowApp()
        {
            Console.WriteLine("Initializing");

            mpu5060 = new Mpu6050(Device.CreateI2cBus());

            TestMpu6050();
        }

        private void TestMpu6050()
        {
            Console.WriteLine("TestMpu6050...");

            // Wake
            mpu5060.Wake();

            while (true)
            {
                Console.WriteLine("Reading...");

                mpu5060.Refresh();
                Console.WriteLine($" ({mpu5060.AccelerationX:X4},{mpu5060.AccelerationY:X4},{mpu5060.AccelerationZ:X4}) ({mpu5060.GyroX:X4},{mpu5060.GyroY:X4},{mpu5060.GyroZ:X4}) {mpu5060.Temperature}");
                
                Thread.Sleep(2000);
            }
        }
    }
}