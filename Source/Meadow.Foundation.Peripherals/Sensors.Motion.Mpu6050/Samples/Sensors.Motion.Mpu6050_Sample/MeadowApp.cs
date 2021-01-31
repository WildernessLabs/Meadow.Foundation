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
                Console.WriteLine($"{mpu.TemperatureC}°C");
                Thread.Sleep(5000);
            }
        }

        private void Mpu_Updated(object sender, Meadow.Peripherals.Sensors.Motion.AccelerationConditionChangeResult e)
        {
            Console.WriteLine($"X: {e.New.XAcceleration}, Y: {e.New.YAcceleration}, Z: {e.New.ZAcceleration}");
        }
    }
}