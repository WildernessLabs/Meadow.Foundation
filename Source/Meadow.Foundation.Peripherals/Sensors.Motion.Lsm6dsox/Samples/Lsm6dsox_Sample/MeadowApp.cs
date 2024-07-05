using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Lsm6dsox_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Lsm6dsox sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize hardware...");
            sensor = new Lsm6dsox(Device.CreateI2cBus());

            // Example that uses an IObservable subscription to only be notified when the filter is satisfied
            var consumer = Lsm6dsox.CreateObserver(handler: result => HandleResult(this, result),
                                                 filter: result => FilterResult(result));

            sensor.Subscribe(consumer);

            // classical .NET events can also be used:
            sensor.Updated += HandleResult;

            sensor.StartUpdating(TimeSpan.FromSeconds(2));

            return base.Initialize();
        }

        bool FilterResult(IChangeResult<(Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D)> result)
        {
            return result.New.Acceleration3D.Value.Z > new Acceleration(0.1, Acceleration.UnitType.Gravity);
        }

        void HandleResult(object sender,
            IChangeResult<(Acceleration3D? Acceleration3D,
            AngularVelocity3D? AngularVelocity3D)> result)
        {
            var accel = result.New.Acceleration3D.GetValueOrDefault();
            var gyro = result.New.AngularVelocity3D.GetValueOrDefault();

            Resolver.Log.Info($"Accelerometer (g): X = {accel.X.Gravity:0.##}, Y = {accel.Y.Gravity:0.##}, Z = {accel.Z.Gravity:0.##}; Gyro (°/s): X = {gyro.X.DegreesPerSecond:0.##}, Y = {gyro.Y.DegreesPerSecond:0.##}, Z = {gyro.Z.DegreesPerSecond:0.##}");
        }

        //<!=SNOP=>
    }
}