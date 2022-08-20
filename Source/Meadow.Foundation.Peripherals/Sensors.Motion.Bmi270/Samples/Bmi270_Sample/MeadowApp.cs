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

            bmi270.Updated += Bmi270_Updated;

            return base.Initialize();
        }

        private void Bmi270_Updated(object sender, 
                                    IChangeResult<(Meadow.Units.Acceleration3D? Acceleration3D, 
                                                   Meadow.Units.AngularVelocity3D? AngularVelocity3D, 
                                                   Meadow.Units.Temperature? Temperature)> e)
        {
            var accel = e.New.Acceleration3D.Value;
            var gyro = e.New.AngularVelocity3D.Value;

            Console.WriteLine($"AX={accel.X.Gravity:0.##}g, AY={accel.Y.Gravity:0.##}g, AZ={accel.Z.Gravity:0.##}g, GX={gyro.X.RadiansPerMinute:0.##}rpm, GY={gyro.Y.RadiansPerMinute:0.##}rpm, GZ={gyro.Z.RadiansPerMinute:0.##}rpm, {e.New.Temperature.Value.Celsius:0.##}C");
        }

        public override Task Run()
        {
            bmi270.StartUpdating(TimeSpan.FromSeconds(1));

            return base.Run();
        }
    }
}