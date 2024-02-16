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
            Resolver.Log.Info("Initialize hardware...");
            bmi270 = new Bmi270(Device.CreateI2cBus());

            // classical .NET events can also be used:
            bmi270.Updated += HandleResult;

            // Example that uses an IObservable subscription to only be notified when the filter is satisfied
            var consumer = Bmi270.CreateObserver(handler: result => HandleResult(this, result),
                                                 filter: result => FilterResult(result));

            bmi270.Subscribe(consumer);

            bmi270.StartUpdating(TimeSpan.FromMilliseconds(2000));

            return base.Initialize();
        }

        bool FilterResult(IChangeResult<(Acceleration3D? Acceleration3D,
                                         AngularVelocity3D? AngularVelocity3D,
                                         Temperature? Temperature)> result)
        {
            return result.New.Acceleration3D.Value.Z > new Acceleration(0.1, Acceleration.UnitType.Gravity);
        }

        void HandleResult(object sender,
            IChangeResult<(Acceleration3D? Acceleration3D,
            AngularVelocity3D? AngularVelocity3D,
            Temperature? Temperature)> result)
        {
            var accel = result.New.Acceleration3D.Value;
            var gyro = result.New.AngularVelocity3D.Value;
            var temp = result.New.Temperature.Value;

            Resolver.Log.Info($"AccelX={accel.X.Gravity:0.##}g, AccelY={accel.Y.Gravity:0.##}g, AccelZ={accel.Z.Gravity:0.##}g, GyroX={gyro.X.RadiansPerMinute:0.##}rpm, GyroY={gyro.Y.RadiansPerMinute:0.##}rpm, GyroZ={gyro.Z.RadiansPerMinute:0.##}rpm, {temp.Celsius:0.##}C");
        }

        //<!=SNOP=>
    }
}