using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Accelerometers;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace MeadowApp
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Bmi270 bmi270;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            bmi270 = new Bmi270(Device.CreateI2cBus());

            // classical .NET events can also be used:
            bmi270.Updated += Bmi270_Updated;

            // Example that uses an IObservable subscription to only be notified when the filter is satisfied
            var consumer = Bmi270.CreateObserver(
                handler: result => Console.WriteLine($"Observer: [x] changed by threshold; new [x]: X:{result.New.Acceleration3D?.X:N2}, old: X:{result.Old?.Acceleration3D?.X:N2}"),
                // only notify if there's a greater than 0.5G change in the Z direction
                filter: result => {
                    if (result.Old is { } old)
                    { //c# 8 pattern match syntax - checks for !null and assigns var
                        return (result.New.Acceleration3D.Value - old.Acceleration3D.Value).Z > new Acceleration(0.5, Acceleration.UnitType.Gravity);
                    }
                    return false;
                });
            bmi270.Subscribe(consumer);

            return Task.CompletedTask;
        }

        private void Bmi270_Updated(object sender, 
                                    IChangeResult<(Acceleration3D? Acceleration3D, 
                                                   AngularVelocity3D? AngularVelocity3D, 
                                                   Temperature? Temperature)> e)
        {
            var accel = e.New.Acceleration3D.Value;
            var gyro = e.New.AngularVelocity3D.Value;

            Console.WriteLine($"AccelX={accel.X.Gravity:0.##}g, AccelY={accel.Y.Gravity:0.##}g, AccelZ={accel.Z.Gravity:0.##}g, GyroX={gyro.X.RadiansPerMinute:0.##}rpm, GyroY={gyro.Y.RadiansPerMinute:0.##}rpm, GyroZ={gyro.Z.RadiansPerMinute:0.##}rpm, {e.New.Temperature.Value.Celsius:0.##}C");
        }

        public override Task Run()
        {
            bmi270.StartUpdating(TimeSpan.FromMilliseconds(500));

            return base.Run();
        }

        //<!=SNOP=>
    }
}