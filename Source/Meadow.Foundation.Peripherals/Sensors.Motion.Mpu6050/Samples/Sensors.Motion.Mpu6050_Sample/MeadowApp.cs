using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace Sensors.Motion.mpu5060_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Mpu6050 mpu;

        public MeadowApp()
        {
            Console.WriteLine("Initializing");

            // Mpu5060 I2C address could be 0x68 or 0x69
            mpu = new Mpu6050(Device.CreateI2cBus(), 0x69);

            mpu.AccelerationChangeThreshold = 0.05f;
            mpu.Updated += Mpu_Updated;
            mpu.StartUpdating(500);

            while (true)
            {
                Console.WriteLine($"{mpu.Temperature.Celsius}°C");
                Thread.Sleep(5000);
            }
        }

        private void Mpu_Updated(object sender, ChangeResult<(Meadow.Units.Acceleration3d Acceleration, Meadow.Units.AngularAcceleration3d AngularAcceleration)> e)
        {
            Console.WriteLine($"X: {e.New.Acceleration.X.MetersPerSecondSquared}, " +
                $"Y: {e.New.Acceleration.Y.MetersPerSecondSquared}, " +
                $"Z: {e.New.Acceleration.Z.MetersPerSecondSquared}");
        }
    }
}