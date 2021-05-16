using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Units;

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
                Console.WriteLine($"{mpu.Temperature.Celsius:n2}C");
                Thread.Sleep(5000);
            }
        }

        private void Mpu_Updated(object sender, IChangeResult<(Acceleration3D? Acceleration, AngularAcceleration3D? AngularAcceleration)> e)
        {
            Console.WriteLine($"Accel: [X:{e.New.Acceleration?.X.MetersPerSecondSquared:N2}," +
                $"Y:{e.New.Acceleration?.Y.MetersPerSecondSquared:N2}," +
                $"Z:{e.New.Acceleration?.Z.MetersPerSecondSquared:N2} (mps^2)]");

            Console.WriteLine($"Angular Accel: [X:{e.New.AngularAcceleration?.X.DegreesPerSecondSquared:N2}," +
                $"Y:{e.New.AngularAcceleration?.Y.DegreesPerSecondSquared:N2}," +
                $"Z:{e.New.AngularAcceleration?.Z.DegreesPerSecondSquared:N2} (dps^2)]");
        }
    }
}