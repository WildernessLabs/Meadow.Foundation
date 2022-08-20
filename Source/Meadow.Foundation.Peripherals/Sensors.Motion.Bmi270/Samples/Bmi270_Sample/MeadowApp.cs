using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Accelerometers;
using System;
using System.Threading.Tasks;

namespace MeadowApp
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV1>
    {
        Bmi270 bmi270;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            bmi270 = new Bmi270(Device.CreateI2cBus());
         //   bmi270.SetAccelerationRange(Bmi270.AccelerationRange._8g);

            bmi270.Updated += Bmi270_Updated;

            return base.Initialize();
        }

        private void Bmi270_Updated(object sender, IChangeResult<(Meadow.Units.Acceleration3D? Acceleration3D, Meadow.Units.AngularVelocity3D? AngularVelocity3D)> e)
        {
            var accel = e.New.Acceleration3D.Value;
            var gyro = e.New.AngularVelocity3D.Value;

            Console.WriteLine($"X={accel.X.Gravity:0.##}g, Y={accel.Y.Gravity:0.##}g, Z={accel.Z.Gravity:0.##}g");
            Console.WriteLine($"X={gyro.X.RadiansPerMinute:0.##}rpm, Y={gyro.Y.RadiansPerMinute:0.##}rpm, Z={gyro.Z.RadiansPerMinute:0.##}rpm");
        }

        public override Task Run()
        {
            bmi270.StartUpdating(TimeSpan.FromSeconds(1));

            return base.Run();
        }
    }
}