using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace Sensors.Motion.mpu5060_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        MPU6050 mpu;

        public MeadowApp()
        {
            Console.WriteLine("Initializing");

            mpu = new MPU6050(Device.CreateI2cBus(), 0x69);

            mpu.AccelerationXChanged += OnRotated;
            mpu.AccelerationYChanged += OnRotated;
            mpu.AccelerationZChanged += OnRotated;
            mpu.AccelerationChangeThreshold = 0.05f;
            mpu.StartSampling(TimeSpan.FromMilliseconds(500));

            while (true)
            {
                Console.WriteLine($"{mpu.TemperatureC}C");
                Thread.Sleep(5000);
            }
        }

        void OnRotated(float before, float after)
        {
            Console.WriteLine($"Acc: ({mpu.AccelerationX}, {mpu.AccelerationY}, {mpu.AccelerationZ})");
        }
    }
}