using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Hardware;
using System;
using System.Threading;

namespace GY521_Sample
{
    public class GY521App : App<F7Micro, GY521App>
    {
        public GY521App()
        {
            Console.WriteLine("+GY521App");

            var i2c = Device.CreateI2cBus();

            GY521Test(i2c);
        }

        private void GY521Test(II2cBus i2c)
        {
            Console.WriteLine("+GY521 Test");

            var gyro = new GY521(i2c);

            Console.WriteLine(" Wake");
            gyro.Wake();

            while (true)
            {
                Console.WriteLine(" Reading...");
                gyro.Refresh();

                Console.WriteLine($" ({gyro.AccelerationX:X4},{gyro.AccelerationY:X4},{gyro.AccelerationZ:X4}) ({gyro.GyroX:X4},{gyro.GyroY:X4},{gyro.GyroZ:X4}) {gyro.Temperature}");

                Thread.Sleep(2000);
            }
        }
    }
}
